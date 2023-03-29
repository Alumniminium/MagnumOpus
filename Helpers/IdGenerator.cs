using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using HerstLib.IO;
using MagnumOpus.ECS;

namespace MagnumOpus.Helpers
{
    public static class IdGenerator
    {
        private static readonly Dictionary<EntityType, ConcurrentStack<int>> Ids;

        static IdGenerator()
        {
            var filename = Path.Combine("_STATE_FILES", $"{nameof(IdGenerator)}.json");

            if (File.Exists(filename))
            {
                var json = File.ReadAllText(filename);
                Ids = JsonSerializer.Deserialize<Dictionary<EntityType, ConcurrentStack<int>>>(json) ?? new();
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

        public static int Get(EntityType type) => Ids[type].TryPop(out var id) ? id : throw new IndexOutOfRangeException(type.ToString());
        public static void Return(EntityType type, int id) => Ids[type].Push(id);

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