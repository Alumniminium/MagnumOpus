using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HerstLib.IO;
using Newtonsoft.Json;

namespace MagnumOpus.ECS
{
    public static class SparseComponentStorage<T> where T : struct
    {
        private static readonly T[] Default = new T[1];
        private static readonly Dictionary<int, T> Components = new();
        private static readonly object lockObj = new();

        static SparseComponentStorage()
        {
            var start = Stopwatch.GetTimestamp();
            var filename = Path.Combine("_STATE_FILES", $"{typeof(T).Name}.json");

            if (!File.Exists(filename))
                return;

            var json = File.ReadAllText(filename);
            Components = JsonConvert.DeserializeObject<Dictionary<int, T>>(json) ?? new();

            // JsonConvert.PopulateObject(json, Components);

            var time = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
            FConsole.WriteLine($"Loaded {typeof(T).Name} in {time}ms");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddFor(in NTT ntt, ref T c)
        {
            lock (lockObj)
            {
                ref var old = ref CollectionsMarshal.GetValueRefOrAddDefault(Components, ntt.Id, out var found);
                old = c;

                if (!found)
                    NttWorld.InformChangesFor(in ntt);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddFor(in NTT ntt)
        {
            lock (lockObj)
            {
                ref var old = ref CollectionsMarshal.GetValueRefOrAddDefault(Components, ntt.Id, out var found);
                if (!found)
                    NttWorld.InformChangesFor(in ntt);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFor(in NTT ntt) => Components.ContainsKey(ntt.Id);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Get(scoped in NTT ntt) => ref Components.ContainsKey(ntt.Id) ? ref CollectionsMarshal.GetValueRefOrNullRef(Components, ntt.Id) : ref Default[0];

        // called via reflection @ ReflectionHelper.Remove<T>()
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Remove(NTT ntt, bool notify)
        {
            lock (lockObj)
            {
                if (!Components.Remove(ntt.Id))
                    return;
            }
            if (notify)
                NttWorld.InformChangesFor(in ntt);
        }

        // called via reflection @ ReflectionHelper.Save<T>()
        public static void Save(string path)
        {
            var start = Stopwatch.GetTimestamp();
            var filename = Path.Combine(path, $"{typeof(T).Name}.json");

            var json = JsonConvert.SerializeObject(Components);
            File.WriteAllText(filename, json);

            var time = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
            FConsole.WriteLine($"Saved {typeof(T).Name} to {filename} in {time}ms");
        }
    }
}