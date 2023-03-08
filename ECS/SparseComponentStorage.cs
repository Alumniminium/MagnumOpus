using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using HerstLib.IO;

namespace MagnumOpus.ECS
{
    public static class SparseComponentStorage<T> where T : struct
    {
        private static readonly T[] Default = new T[1];
        private static readonly Dictionary<int, T> NttToComponents = new();
        private static readonly object lockObj = new();

        static SparseComponentStorage()
        {
            var start = Stopwatch.GetTimestamp();
            var filename = "_STATE_FILES/" + typeof(T).Name + ".json";
            if (!File.Exists(filename))
                return;
            
            using var stream = File.OpenRead(filename);
            NttToComponents = JsonSerializer.Deserialize<Dictionary<int,T>>(stream) ?? new ();
            var time = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
            FConsole.WriteLine($"Loaded {typeof(T).Name} in {time}ms");
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddFor(in NTT owner, ref T component)
        {
            lock (lockObj)
            {
                ref var existing = ref CollectionsMarshal.GetValueRefOrAddDefault(NttToComponents, owner.Id, out var exists);
                existing = component;

                if (!exists)
                    NttWorld.InformChangesFor(in owner);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFor(in NTT owner) => NttToComponents.ContainsKey(owner.Id);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Get(scoped in NTT owner)
        {
            if (NttToComponents.ContainsKey(owner.Id))
                return ref CollectionsMarshal.GetValueRefOrNullRef(NttToComponents, owner.Id);
            return ref Default[0];
        }

        // called via reflection @ ReflectionHelper.Remove<T>()
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Remove(NTT owner, bool notify)
        {
            lock (lockObj)
            {
                if (!NttToComponents.Remove(owner.Id))
                    return;
            }
            if (notify)
                NttWorld.InformChangesFor(in owner);
        }

        // called via reflection @ ReflectionHelper.Save<T>()
        private static readonly JsonSerializerOptions serializerOptions = new()
        {
            IncludeFields=true,
            IgnoreReadOnlyFields = false,
            WriteIndented = false
        };
        public static void Save(string path)
        {
            var start = Stopwatch.GetTimestamp();
            using var stream = File.OpenWrite(path + "/" + typeof(T).Name + ".json");
            using var writer = new Utf8JsonWriter(stream);
            JsonSerializer.Serialize(stream, NttToComponents, serializerOptions);
            var time = Stopwatch.GetElapsedTime(start).TotalMilliseconds;

            FConsole.WriteLine($"Saved {typeof(T).Name} to {path} in {time}ms");
        }
    }
}