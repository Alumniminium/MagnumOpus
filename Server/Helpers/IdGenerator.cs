using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using MagnumOpus.IO;

namespace MagnumOpus.Helpers;

/// <summary>
/// Thread-safe ID generation and management system for different entity types.
/// Provides unique ID allocation with persistence and recycling capabilities for game entities.
/// </summary>
public static class IdGenerator
{
    private static readonly ConcurrentStack<int> PlayerIds;
    private static readonly ConcurrentStack<int> MonsterIds;
    private static readonly ConcurrentStack<int> NpcIds;
    private static readonly ConcurrentStack<int> ItemIds;
    private static readonly ConcurrentStack<int> TrapIds;
    private static readonly ConcurrentStack<int> OtherIds;

    /// <summary>
    /// Initializes the ID generator by loading existing state or creating default ID ranges for each entity type.
    /// </summary>
    static IdGenerator()
    {
        var filename = Path.Combine("_STATE_FILES", $"{nameof(IdGenerator)}.json");

        if (File.Exists(filename))
        {
            var json = File.ReadAllText(filename);
            var data = JsonSerializer.Deserialize<IdGeneratorState>(json);
            if (data is not null)
            {
                NpcIds = data.NpcIds ?? new(Enumerable.Range(0, 399_999));
                MonsterIds = data.MonsterIds ?? new(Enumerable.Range(400_000, 399_999));
                PlayerIds = data.PlayerIds ?? new(Enumerable.Range(1_000_000, 1_000_000));
                ItemIds = data.ItemIds ?? new(Enumerable.Range(2_000_000, 1_000_000));
                TrapIds = data.TrapIds ?? new(Enumerable.Range(800_000, 100_000));
                OtherIds = data.OtherIds ?? new(Enumerable.Range(3_000_000, 1_000_000));
                return;
            }
        }

        NpcIds = new(Enumerable.Range(0, 399_999));
        MonsterIds = new(Enumerable.Range(400_000, 399_999));
        PlayerIds = new(Enumerable.Range(1_000_000, 100_000));
        ItemIds = new(Enumerable.Range(2_000_000, 1_000_000));
        TrapIds = new(Enumerable.Range(800_000, 100_000));
        OtherIds = new(Enumerable.Range(3_000_000, 1_000_000));
    }

    /// <summary>
    /// Allocates a unique ID for a player entity.
    /// </summary>
    /// <returns>Unique ID for the player</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown when no IDs are available</exception>
    public static int GetPlayerId() => PlayerIds.TryPop(out var id) ? id : throw new IndexOutOfRangeException("Player IDs exhausted");

    /// <summary>
    /// Allocates a unique ID for a monster entity.
    /// </summary>
    /// <returns>Unique ID for the monster</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown when no IDs are available</exception>
    public static int GetMonsterId() => MonsterIds.TryPop(out var id) ? id : throw new IndexOutOfRangeException("Monster IDs exhausted");

    /// <summary>
    /// Allocates a unique ID for an NPC entity.
    /// </summary>
    /// <returns>Unique ID for the NPC</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown when no IDs are available</exception>
    public static int GetNpcId() => NpcIds.TryPop(out var id) ? id : throw new IndexOutOfRangeException("NPC IDs exhausted");

    /// <summary>
    /// Allocates a unique ID for an item entity.
    /// </summary>
    /// <returns>Unique ID for the item</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown when no IDs are available</exception>
    public static int GetItemId() => ItemIds.TryPop(out var id) ? id : throw new IndexOutOfRangeException("Item IDs exhausted");

    /// <summary>
    /// Allocates a unique ID for a trap entity.
    /// </summary>
    /// <returns>Unique ID for the trap</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown when no IDs are available</exception>
    public static int GetTrapId() => TrapIds.TryPop(out var id) ? id : throw new IndexOutOfRangeException("Trap IDs exhausted");

    /// <summary>
    /// Allocates a unique ID for an "other" type entity.
    /// </summary>
    /// <returns>Unique ID for the other entity</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown when no IDs are available</exception>
    public static int GetOtherId() => OtherIds.TryPop(out var id) ? id : throw new IndexOutOfRangeException("Other IDs exhausted");

    /// <summary>
    /// Returns a player ID back to the pool for reuse.
    /// </summary>
    /// <param name="id">ID to return to the pool</param>
    public static void ReturnPlayerId(int id) => PlayerIds.Push(id);

    /// <summary>
    /// Returns a monster ID back to the pool for reuse.
    /// </summary>
    /// <param name="id">ID to return to the pool</param>
    public static void ReturnMonsterId(int id) => MonsterIds.Push(id);

    /// <summary>
    /// Returns an NPC ID back to the pool for reuse.
    /// </summary>
    /// <param name="id">ID to return to the pool</param>
    public static void ReturnNpcId(int id) => NpcIds.Push(id);

    /// <summary>
    /// Returns an item ID back to the pool for reuse.
    /// </summary>
    /// <param name="id">ID to return to the pool</param>
    public static void ReturnItemId(int id) => ItemIds.Push(id);

    /// <summary>
    /// Returns a trap ID back to the pool for reuse.
    /// </summary>
    /// <param name="id">ID to return to the pool</param>
    public static void ReturnTrapId(int id) => TrapIds.Push(id);

    /// <summary>
    /// Returns an "other" ID back to the pool for reuse.
    /// </summary>
    /// <param name="id">ID to return to the pool</param>
    public static void ReturnOtherId(int id) => OtherIds.Push(id);

    /// <summary>
    /// Persists the current ID generator state to disk for recovery after server restart.
    /// </summary>
    /// <param name="path">Directory path to save the ID generator state file</param>
    public static void Save(string path)
    {
        var start = Stopwatch.GetTimestamp();
        var filename = Path.Combine(path, $"{nameof(IdGenerator)}.json");

        var state = new IdGeneratorState
        {
            NpcIds = NpcIds,
            MonsterIds = MonsterIds,
            PlayerIds = PlayerIds,
            ItemIds = ItemIds,
            TrapIds = TrapIds,
            OtherIds = OtherIds
        };

        using var stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
        JsonSerializer.Serialize(stream, state, Constants.serializerOptions);

        var time = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
        FConsole.WriteLine($"Saved {nameof(IdGenerator)} to {filename} in {time}ms");
    }

    private class IdGeneratorState
    {
        public ConcurrentStack<int>? NpcIds { get; set; }
        public ConcurrentStack<int>? MonsterIds { get; set; }
        public ConcurrentStack<int>? PlayerIds { get; set; }
        public ConcurrentStack<int>? ItemIds { get; set; }
        public ConcurrentStack<int>? TrapIds { get; set; }
        public ConcurrentStack<int>? OtherIds { get; set; }
    }
}