using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using MagnumOpus.IO;

namespace NttECS.ECS;

/// <summary>
/// High-performance packed storage system for ECS components providing excellent cache locality.
/// Uses dense arrays for component storage with entity-to-index mapping for O(1) access.
/// Components are stored contiguously in memory enabling efficient system iteration and vectorization.
/// </summary>
/// <typeparam name="T">Component type to store (must be value type)</typeparam>
public static class PackedComponentStorage<T> where T : struct
{
    /// <summary>Dense array storing components contiguously for optimal cache performance</summary>
    private static T[] _components = new T[1024];

    /// <summary>Maps entity IDs to component array indices for O(1) lookups</summary>
    private static readonly int[] _entityToIndex = new int[4_000_000];

    /// <summary>Maps component array indices back to entity IDs for iteration</summary>
    private static int[] _indexToEntity = new int[1024];

    /// <summary>Current number of components stored</summary>
    private static int _count = 0;

    /// <summary>Indicates which entity slots are occupied (entity ID -> bool)</summary>
    private static readonly bool[] _hasComponent = new bool[4_000_000];

    /// <summary>Reader-writer lock for thread-safe concurrent access</summary>
    private static readonly ReaderWriterLockSlim _lock = new();

    /// <summary>Default component instance returned when component doesn't exist</summary>
    private static readonly T[] _default = new T[1];

    /// <summary>
    /// Adds or updates a component for the specified entity with optimal cache placement.
    /// Components are packed together for excellent system iteration performance.
    /// </summary>
    /// <param name="ntt">Entity to add component to</param>
    /// <param name="component">Component data to store</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddFor(in NTT ntt, ref T component)
    {
        if (ntt.Id == 0 || ntt.Id >= _hasComponent.Length)
            return;

        _lock.EnterWriteLock();
        try
        {
            if (_hasComponent[ntt.Id])
            {
                // Update existing component
                var index = _entityToIndex[ntt.Id];
                _components[index] = component;
            }
            else
            {
                // Add new component
                EnsureCapacity(_count + 1);

                var index = _count;
                _components[index] = component;
                _entityToIndex[ntt.Id] = index;
                _indexToEntity[index] = ntt.Id;
                _hasComponent[ntt.Id] = true;
                _count++;

                NttWorld.InformChangesFor(ntt);
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Adds a default-initialized component for the specified entity.
    /// </summary>
    /// <param name="ntt">Entity to add default component to</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddFor(in NTT ntt)
    {
        var defaultComponent = default(T);
        AddFor(ntt, ref defaultComponent);
    }

    /// <summary>
    /// Checks if the specified entity has this component type.
    /// </summary>
    /// <param name="ntt">Entity to check for component</param>
    /// <returns>True if entity has the component</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasFor(in NTT ntt)
    {
        if (ntt.Id == 0 || ntt.Id >= _hasComponent.Length)
            return false;

        _lock.EnterReadLock();
        try
        {
            return _hasComponent[ntt.Id];
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Gets a mutable reference to the component for the specified entity.
    /// Returns a reference to default component if entity doesn't have this component type.
    /// </summary>
    /// <param name="ntt">Entity to get component for</param>
    /// <returns>Mutable reference to component data</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T Get(NTT ntt)
    {
        if (ntt.Id == 0 || ntt.Id >= _hasComponent.Length)
            return ref _default[0];

        _lock.EnterReadLock();
        try
        {
            if (!_hasComponent[ntt.Id])
                return ref _default[0];

            var index = _entityToIndex[ntt.Id];
            return ref _components[index];
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Removes the component from the specified entity with optimal array compaction.
    /// Maintains component array density by moving the last component to fill the gap.
    /// </summary>
    /// <param name="ntt">Entity to remove component from</param>
    /// <param name="notify">Whether to notify world of entity changes</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Remove(NTT ntt, bool notify)
    {
        if (ntt.Id == 0 || ntt.Id >= _hasComponent.Length)
            return;

        _lock.EnterWriteLock();
        try
        {
            if (!_hasComponent[ntt.Id])
                return;

            var indexToRemove = _entityToIndex[ntt.Id];
            var lastIndex = _count - 1;

            if (indexToRemove != lastIndex)
            {
                // Move last component to fill the gap (maintain density)
                var lastEntityId = _indexToEntity[lastIndex];
                _components[indexToRemove] = _components[lastIndex];
                _entityToIndex[lastEntityId] = indexToRemove;
                _indexToEntity[indexToRemove] = lastEntityId;
            }

            // Clear the last slot
            _components[lastIndex] = default;
            _indexToEntity[lastIndex] = 0;
            _hasComponent[ntt.Id] = false;
            _count--;

            if (notify)
                NttWorld.InformChangesFor(ntt);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Transfers component ownership from one entity to another atomically.
    /// </summary>
    /// <param name="from">Source entity to transfer component from</param>
    /// <param name="to">Target entity to transfer component to</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ChangeOwner(NTT from, NTT to)
    {
        if (from.Id == 0 || to.Id == 0 || from.Id >= _hasComponent.Length || to.Id >= _hasComponent.Length)
            return;

        _lock.EnterWriteLock();
        try
        {
            if (!_hasComponent[from.Id])
                return;

            var index = _entityToIndex[from.Id];
            var component = _components[index];

            // Remove from source
            _hasComponent[from.Id] = false;

            // Add to target
            _entityToIndex[to.Id] = index;
            _indexToEntity[index] = to.Id;
            _hasComponent[to.Id] = true;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Gets a read-only span of all components for efficient system iteration.
    /// Provides excellent cache locality for processing all components of this type.
    /// </summary>
    /// <returns>Read-only span covering all active components</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<T> GetComponentSpan()
    {
        _lock.EnterReadLock();
        try
        {
            return _components.AsSpan(0, _count);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Gets a span of all entity IDs that have this component type.
    /// Parallel to GetComponentSpan() for entity-component iteration.
    /// </summary>
    /// <returns>Read-only span of entity IDs with this component</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<int> GetEntitySpan()
    {
        _lock.EnterReadLock();
        try
        {
            return _indexToEntity.AsSpan(0, _count);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Gets the current number of entities with this component type.
    /// </summary>
    /// <returns>Number of active components</returns>
    public static int Count
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _count;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    /// <summary>
    /// Ensures the component arrays have sufficient capacity for the specified count.
    /// Grows arrays exponentially to minimize allocations.
    /// </summary>
    /// <param name="requiredCapacity">Minimum required capacity</param>
    private static void EnsureCapacity(int requiredCapacity)
    {
        if (requiredCapacity <= _components.Length)
            return;

        var newCapacity = Math.Max(requiredCapacity, _components.Length * 2);
        Array.Resize(ref _components, newCapacity);
        Array.Resize(ref _indexToEntity, newCapacity);
    }

    /// <summary>
    /// Saves all components of this type to disk for persistence between server restarts.
    /// </summary>
    /// <param name="path">Directory path to save component data</param>
    public static void Save(string path)
    {
        var start = Stopwatch.GetTimestamp();
        var filename = Path.Combine(path, $"{typeof(T).Name}.json");

        _lock.EnterReadLock();
        try
        {
            // Create a dictionary for JSON serialization compatibility
            var componentDict = new Dictionary<int, T>();
            for (var i = 0; i < _count; i++)
            {
                componentDict[_indexToEntity[i]] = _components[i];
            }

            var json = JsonSerializer.Serialize(componentDict);
            File.WriteAllText(filename, json);
        }
        finally
        {
            _lock.ExitReadLock();
        }

        var time = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
        FConsole.WriteLine($"Saved {typeof(T).Name} ({_count} components) to {filename} in {time}ms");
    }

    /// <summary>
    /// Loads component data from disk, restoring component state from previous server session.
    /// </summary>
    /// <param name="path">Directory path to load component data from</param>
    public static void Load(string path)
    {
        var start = Stopwatch.GetTimestamp();
        var filename = Path.Combine(path, $"{typeof(T).Name}.json");

        if (!File.Exists(filename))
            return;

        _lock.EnterWriteLock();
        try
        {
            var json = File.ReadAllText(filename);
            var componentDict = JsonSerializer.Deserialize<Dictionary<int, T>>(json) ?? [];

            // Clear existing data
            _count = 0;
            Array.Clear(_hasComponent);

            // Load components into packed arrays
            foreach (var kvp in componentDict)
            {
                var entityId = kvp.Key;
                var component = kvp.Value;

                if (entityId >= _hasComponent.Length)
                    continue;

                EnsureCapacity(_count + 1);

                _components[_count] = component;
                _entityToIndex[entityId] = _count;
                _indexToEntity[_count] = entityId;
                _hasComponent[entityId] = true;
                _count++;
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        var time = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
        FConsole.WriteLine($"Loaded {typeof(T).Name} ({_count} components) in {time}ms");
    }
}