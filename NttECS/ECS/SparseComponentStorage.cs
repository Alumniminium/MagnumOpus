using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using MagnumOpus.IO;

namespace NttECS.ECS;

/// <summary>
/// Thread-safe sparse storage system for ECS components providing efficient memory usage and fast access.
/// Uses a generic dictionary-based approach with reader-writer locks for concurrent access safety.
/// Only stores components for entities that actually have them, avoiding memory waste for sparse data.
/// </summary>
/// <typeparam name="T">Component type to store (must be value type)</typeparam>
public static class SparseComponentStorage<T> where T : struct
{
    /// <summary>Default component instance returned when component doesn't exist</summary>
    private static readonly T[] Default = new T[1];
    /// <summary>Core storage mapping entity IDs to component instances</summary>
    private static readonly Dictionary<int, T> Components = [];
    /// <summary>Reader-writer lock for thread-safe concurrent access</summary>
    private static readonly ReaderWriterLockSlim lockObj = new();

    /// <summary>
    /// Adds or updates a component for the specified entity with thread-safe write access.
    /// Notifies the world of entity changes if this is a new component addition.
    /// </summary>
    /// <param name="ntt">Entity to add component to</param>
    /// <param name="c">Component data to store</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddFor(in NTT ntt, ref T c)
    {
        if (ntt.Id == 0)
            return;

        lockObj.EnterWriteLock();
        try
        {
            ref var old = ref CollectionsMarshal.GetValueRefOrAddDefault(Components, ntt.Id, out var found);
            old = c;

            if (!found)
                NttWorld.InformChangesFor(ntt);
        }
        finally
        {
            lockObj.ExitWriteLock();
        }
    }
    /// <summary>
    /// Adds a default-initialized component for the specified entity.
    /// Useful for marker components or when default values are sufficient.
    /// </summary>
    /// <param name="ntt">Entity to add default component to</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddFor(in NTT ntt)
    {
        if (ntt.Id == 0)
            return;

        lockObj.EnterWriteLock();
        try
        {
            ref var old = ref CollectionsMarshal.GetValueRefOrAddDefault(Components, ntt.Id, out var found);
            if (!found)
                NttWorld.InformChangesFor(ntt);
        }
        finally
        {
            lockObj.ExitWriteLock();
        }
    }
    /// <summary>
    /// Checks if the specified entity has this component type with thread-safe read access.
    /// </summary>
    /// <param name="ntt">Entity to check for component</param>
    /// <returns>True if entity has the component</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasFor(in NTT ntt)
    {
        lockObj.EnterReadLock();
        try
        {
            return Components?.ContainsKey(ntt.Id) ?? false;
        }
        finally
        {
            lockObj.ExitReadLock();
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
        lockObj.EnterReadLock();
        try
        {
            ref var t = ref CollectionsMarshal.GetValueRefOrNullRef(Components, ntt.Id);
            return ref Unsafe.IsNullRef(ref t) ? ref Default[0] : ref t;
        }
        finally
        {
            lockObj.ExitReadLock();
        }
    }

    /// <summary>
    /// Removes the component from the specified entity with optional change notification.
    /// Called via reflection from ReflectionHelper.Remove&lt;T&gt;() for type-safe removal.
    /// </summary>
    /// <param name="ntt">Entity to remove component from</param>
    /// <param name="notify">Whether to notify world of entity changes</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Remove(NTT ntt, bool notify)
    {
        if (ntt.Id == 0)
            return;

        lockObj.EnterWriteLock();
        try
        {
            if (!Components.Remove(ntt.Id))
                return;
            if (notify)
                NttWorld.InformChangesFor(ntt);
        }
        finally
        {
            lockObj.ExitWriteLock();
        }
    }

    /// <summary>
    /// Transfers component ownership from one entity to another atomically.
    /// Used for entity ownership changes while preserving component data.
    /// </summary>
    /// <param name="from">Source entity to transfer component from</param>
    /// <param name="to">Target entity to transfer component to</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ChangeOwner(NTT from, NTT to)
    {
        lockObj.EnterWriteLock();
        try
        {
            if (Components.Remove(from.Id, out var c))
                Components.TryAdd(to.Id, c);
        }
        finally
        {
            lockObj.ExitWriteLock();
        }
    }

    /// <summary>
    /// Saves all components of this type to disk for persistence between server restarts.
    /// Called via reflection from ReflectionHelper.Save&lt;T&gt;() for type-safe serialization.
    /// </summary>
    /// <param name="path">Directory path to save component data</param>
    public static void Save(string path)
    {
        var start = Stopwatch.GetTimestamp();
        var filename = Path.Combine(path, $"{typeof(T).Name}.json");

        var json = JsonSerializer.Serialize(Components);
        File.WriteAllText(filename, json);

        var time = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
        FConsole.WriteLine($"Saved {typeof(T).Name} to {filename} in {time}ms");
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

        var json = File.ReadAllText(filename);
        var components = JsonSerializer.Deserialize<Dictionary<int, T>>(json) ?? [];
        foreach (var kvp in components)
            Components.Add(kvp.Key, kvp.Value);

        var time = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
        FConsole.WriteLine($"Loaded {typeof(T).Name} in {time}ms");
    }
}