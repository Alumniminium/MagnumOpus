using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using HerstLib.IO;

namespace MagnumOpus.ECS
{
    public static class ComponentList<T> where T : struct
    {
        private static readonly JsonSerializerOptions SerializerOptions = new ()
        {
            WriteIndented = false,
            IgnoreNullValues = true,
            IncludeFields = true,
        };

        private static readonly T[] Array = new T[NttWorld.MaxEntities];

        static ComponentList()
        {
            var start = Stopwatch.GetTimestamp();
            var filename = "_STATE_FILES/" + typeof(T).Name + ".json";
            if (!File.Exists(filename))
                return;
            
            using var stream = File.OpenRead(filename);
            Array = JsonSerializer.Deserialize<T[]>(stream);
            var time = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
            FConsole.WriteLine($"Loaded {typeof(T).Name} in {time}ms");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddFor(in NTT owner, ref T component)
        {
            Array[owner.Id] = component;
            NttWorld.InformChangesFor(in owner);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFor(in NTT owner) => Array[owner.Id].GetHashCode() != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Get(NTT owner) => ref Array[owner.Id];
        // called via reflection @ ReflectionHelper.Remove<T>()
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Remove(NTT owner, bool notify)
        {
            Array[owner.Id] = default;
            if (notify)
                NttWorld.InformChangesFor(in owner);
        }

        // called via reflection @ ReflectionHelper.Save<T>()
        public static void Save(string path)
        {
            var start = Stopwatch.GetTimestamp();
            using var stream = File.OpenWrite(path + "/" + typeof(T).Name + ".json");
            using var writer = new Utf8JsonWriter(stream);
            JsonSerializer.Serialize(stream, Array, SerializerOptions);
            var time = Stopwatch.GetElapsedTime(start).TotalMilliseconds;

            FConsole.WriteLine($"Saved {typeof(T).Name} to {path} in {time}ms");
        }
    }
}