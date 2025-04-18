using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HerstLib.IO;
using MagnumOpus.Helpers;
using Newtonsoft.Json;

namespace MagnumOpus.ECS
{
    public static class NttWorld
    {
        public static int TargetTps { get; private set; } = 60;
        private static float UpdateTime => 1f / TargetTps;
        public static int EntityCount => NTTs.Count;

        private static readonly NTT[] Default = new NTT[1];
        public static readonly Dictionary<int, NTT> NTTs = new();
        public static readonly HashSet<NTT> Players = new();

        private static readonly ConcurrentQueue<NTT> ToBeRemoved = new();
        public static readonly ConcurrentQueue<NTT> ChangedThisTick = new();

        private static NttSystem[] Systems = [];
        public static long Tick { get; private set; }
        private static long TickBeginTime;
        private static float TimeAcc;
        private static float UpdateTimeAcc;

        private static Action? OnSecond;
        private static Action? OnEndTick;

        static NttWorld()
        {
            Default[0] = new(0, EntityType.Other);
            var start = Stopwatch.GetTimestamp();
            var filename = Path.Combine("_STATE_FILES", "NttWorld.json");

            if (!File.Exists(filename))
            {
                NTTs = new();
                return;
            }

            if (File.Exists("_STATE_FILES/tick.last"))
                Tick = long.Parse(File.ReadAllText("_STATE_FILES/tick.last"));

            var json = File.ReadAllText(filename);
            NTTs = JsonConvert.DeserializeObject<Dictionary<int, NTT>>(json) ?? new();
            PrometheusPush.NTTCount.IncTo(NTTs.Count);
            var time = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
            FConsole.WriteLine($"Loaded NttWorld in {time}ms");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetSystems(params NttSystem[] systems) => Systems = systems;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetTPS(int fps) => TargetTps = fps;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RegisterOnSecond(Action action) => OnSecond += action;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RegisterOnEndTick(Action action) => OnEndTick += action;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref NTT CreateEntity(EntityType type) => ref CreateEntityWithNetId(type, IdGenerator.Get(type));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref NTT CreateEntityWithNetId(EntityType type, int id = 0)
        {
            var ntt = new NTT(id, type);
            NTTs.Add(ntt.Id, ntt);
            PrometheusPush.NTTCount.Inc();
            PrometheusPush.NTTCreations.Inc();
            return ref CollectionsMarshal.GetValueRefOrNullRef(NTTs, id);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref NTT GetEntity(int nttId) => ref NTTs.ContainsKey(nttId) ? ref CollectionsMarshal.GetValueRefOrNullRef(NTTs, nttId) : ref Default[0];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EntityExists(int nttId) => NTTs.ContainsKey(nttId);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InformChangesFor(NTT ntt) => ChangedThisTick.Enqueue(ntt);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Destroy(NTT ntt) => ToBeRemoved.Enqueue(ntt);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DestroyInternal(NTT ntt)
        {
            Players.Remove(ntt);
            ntt.Recycle();
            ChangedThisTick.Enqueue(ntt);
            NTTs.Remove(ntt.Id);

            PrometheusPush.NTTCount.Set(EntityCount);
            PrometheusPush.NTTDestroys.Inc();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UpdateNTTs()
        {
            while (ToBeRemoved.TryDequeue(out var ntt))
                DestroyInternal(ntt);

            while (ChangedThisTick.TryDequeue(out var ntt))
            {
                PrometheusPush.NTTChanges.Inc();
                foreach (var system in Systems)
                    system.EntityChanged(ntt);
            }
        }
        public static void Update()
        {
            var tickTime = Stopwatch.GetElapsedTime(TickBeginTime);
            TickBeginTime = Stopwatch.GetTimestamp();
            var dt = MathF.Min(1f / TargetTps, (float)tickTime.TotalSeconds);
            TimeAcc += dt;
            UpdateTimeAcc += dt;

            if (UpdateTimeAcc >= UpdateTime)
            {
                UpdateTimeAcc -= UpdateTime;

                for (var i = 0; i < Systems.Length; i++)
                {
                    UpdateNTTs();
                    Systems[i].BeginUpdate();
                }
                UpdateNTTs();

                OnEndTick?.Invoke();
                Tick++;
                PrometheusPush.TickCount.Inc();

                if (TimeAcc < 1)
                    return;

                OnSecond?.Invoke();
                TimeAcc = 0;
            }

            var tickDuration = (float)Stopwatch.GetElapsedTime(TickBeginTime).TotalMilliseconds;
            PrometheusPush.TickTime.Observe(tickDuration);
            var sleepTime = (int)Math.Max(0, -1 + (UpdateTime * 1000) - tickDuration);
            Thread.Sleep(sleepTime);
        }

        public static void Save(string path)
        {
            var start = Stopwatch.GetTimestamp();
            var filename = Path.Combine(path, $"{nameof(NttWorld)}.json");
            var json = JsonConvert.SerializeObject(NTTs);
            File.WriteAllText(filename, json);
            File.WriteAllText(path + "/tick.last", $"{Tick}");

            var time = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
            FConsole.WriteLine($"Saved {nameof(NttWorld)} to {filename} in {time}ms");
        }
    }
}