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
        private static readonly Dictionary<int, T> Components = [];
        private static readonly ReaderWriterLockSlim lockObj = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddFor(in NTT ntt, ref T c)
        {
            if (ntt.Id == 0)
                return;

            lock (lockObj)
            {
                ref var old = ref CollectionsMarshal.GetValueRefOrAddDefault(Components, ntt.Id, out var found);
                old = c;

                if (!found)
                    NttWorld.InformChangesFor(ntt);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddFor(in NTT ntt)
        {
            if (ntt.Id == 0)
                return;

            lock (lockObj)
            {
                ref var old = ref CollectionsMarshal.GetValueRefOrAddDefault(Components, ntt.Id, out var found);
                if (!found)
                    NttWorld.InformChangesFor(ntt);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFor(in NTT ntt) => Components?.ContainsKey(ntt.Id) ?? false;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Get(NTT ntt)
        {
            lock (lockObj)
            {
                ref var t = ref CollectionsMarshal.GetValueRefOrNullRef(Components, ntt.Id);
                return ref Unsafe.IsNullRef(ref t) ? ref Default[0] : ref t;
            }
        }

        // called via reflection @ ReflectionHelper.Remove<T>()
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Remove(NTT ntt, bool notify)
        {
            if (ntt.Id == 0)
                return;

            lock (lockObj)
            {
                if (!Components.Remove(ntt.Id))
                    return;
                if (notify)
                    NttWorld.InformChangesFor(ntt);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ChangeOwner(NTT from, NTT to)
        {
            lock (lockObj)
            {
                if (Components.Remove(from.Id, out var c))
                    Components.TryAdd(to.Id, c);
            }
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

        public static void Load(string path)
        {
            var start = Stopwatch.GetTimestamp();
            var filename = Path.Combine(path, $"{typeof(T).Name}.json");

            if (!File.Exists(filename))
                return;

            var json = File.ReadAllText(filename);
            var components = JsonConvert.DeserializeObject<Dictionary<int, T>>(json) ?? [];
            foreach (var kvp in components)
                Components.Add(kvp.Key, kvp.Value);
            // JsonConvert.PopulateObject(json, Components);

            var time = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
            FConsole.WriteLine($"Loaded {typeof(T).Name} in {time}ms");
        }
    }
}