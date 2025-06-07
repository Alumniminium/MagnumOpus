using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using HerstLib.IO;
using MagnumOpus.ECS;

namespace MagnumOpus.Helpers
{
    /// <summary>
    /// Thread-safe ID generation and management system for different entity types.
    /// Provides unique ID allocation with persistence and recycling capabilities for game entities.
    /// </summary>
    public static class IdGenerator
    {
        private static readonly Dictionary<EntityType, ConcurrentStack<int>> Ids;

        /// <summary>
        /// Initializes the ID generator by loading existing state or creating default ID ranges for each entity type.
        /// </summary>
        static IdGenerator()
        {
            var filename = Path.Combine("_STATE_FILES", $"{nameof(IdGenerator)}.json");

            if (File.Exists(filename))
            {
                var json = File.ReadAllText(filename);
                Ids = JsonSerializer.Deserialize<Dictionary<EntityType, ConcurrentStack<int>>>(json) ?? [];
                return;
            }

            Ids = new()
            {
                [EntityType.Npc] = new(Enumerable.Range(0, 399_999)),
                [EntityType.Monster] = new(Enumerable.Range(400_000, 399_999)),
                [EntityType.Player] = new(Enumerable.Range(1_000_000, 100_000)),
                [EntityType.Item] = new(Enumerable.Range(2_000_000, 1_000_000)),
                [EntityType.Other] = new(Enumerable.Range(3_000_000, 1_000_000)),
            };
        }

        /// <summary>
        /// Allocates a unique ID for the specified entity type from the available pool.
        /// </summary>
        /// <param name="type">Entity type requiring an ID</param>
        /// <returns>Unique ID for the entity type</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown when no IDs are available for the entity type</exception>
        public static int Get(EntityType type) => Ids[type].TryPop(out var id) ? id : throw new IndexOutOfRangeException(type.ToString());
        
        /// <summary>
        /// Returns an ID back to the pool for reuse when an entity is destroyed.
        /// </summary>
        /// <param name="type">Entity type of the returned ID</param>
        /// <param name="id">ID to return to the pool</param>
        public static void Return(EntityType type, int id) => Ids[type].Push(id);

        /// <summary>
        /// Persists the current ID generator state to disk for recovery after server restart.
        /// </summary>
        /// <param name="path">Directory path to save the ID generator state file</param>
        public static void Save(string path)
        {
            var start = Stopwatch.GetTimestamp();
            var filename = Path.Combine(path, $"{nameof(IdGenerator)}.json");

            using var stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
            JsonSerializer.Serialize(stream, Ids, Constants.serializerOptions);

            var time = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
            FConsole.WriteLine($"Saved {nameof(IdGenerator)} to {filename} in {time}ms");
        }
    }
}