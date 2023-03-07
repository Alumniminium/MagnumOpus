using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text.Json;
using HerstLib.IO;
using MagnumOpus.Helpers;

namespace MagnumOpus.ECS
{
    public static class NttWorld
    {
        public static int TargetTps { get; private set; } = 60;
        private static float UpdateTime => 1f / TargetTps;

        public static readonly int MaxEntities = 500_000;
        public static int EntityCount => MaxEntities - AvailableArrayIndicies.Count;

        private static readonly NTT[] Entities;
        private static readonly Dictionary<int, int> NetIdToEntityIndex = new();
        private static readonly ConcurrentQueue<int> AvailableArrayIndicies;
        public static readonly HashSet<NTT> Players = new();

        private static readonly ConcurrentQueue<NTT> ToBeRemoved = new();
        public static readonly ConcurrentQueue<NTT> ChangedThisTick = new();

        private static NttSystem[] Systems;
        public static long Tick { get; private set; }
        private static long TickBeginTime;
        private static float TimeAcc;
        private static float UpdateTimeAcc;

        private static SystemNotifier? SystemNotifier;

        private static Action? OnSecond;
        private static Action? OnEndTick;

        static NttWorld()
        {
            GCSettings.LatencyMode = GCLatencyMode.Interactive;
            Systems = Array.Empty<NttSystem>();
            // PerformanceMetrics.RegisterSystem(nameof(NttWorld));
            Entities = new NTT[MaxEntities];
            AvailableArrayIndicies = new();

            for (var i = 0; i < Entities.Length; i++)
                AvailableArrayIndicies.Enqueue(i);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetSystems(params NttSystem[] systems)
        {
            Systems = systems;
            SystemNotifier = new SystemNotifier(systems);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetTPS(int fps) => TargetTps = fps;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RegisterOnSecond(Action action) => OnSecond += action;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RegisterOnEndTick(Action action) => OnEndTick += action;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref NTT CreateEntity(EntityType type)
        {
            lock (Entities)
            {
                if (AvailableArrayIndicies.TryDequeue(out var arrayIndex))
                {
                    var netId = IdGenerator.Get(type);
                    Entities[arrayIndex] = new NTT(arrayIndex, netId, type);
                    NetIdToEntityIndex.Add(netId, arrayIndex);
                    PrometheusPush.NTTCount.Inc();
                    PrometheusPush.NTTCreations.Inc();
                    return ref Entities[arrayIndex];
                }
            }
            throw new IndexOutOfRangeException("Failed to pop an array index");
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref NTT CreateEntityWithNetId(EntityType type, int netId = 0)
        {
            lock (Entities)
            {
                if (AvailableArrayIndicies.TryDequeue(out var arrayIndex))
                {
                    Entities[arrayIndex] = new NTT(arrayIndex, netId, type);
                    NetIdToEntityIndex.Add(netId, arrayIndex);
                    PrometheusPush.NTTCount.Inc();
                    PrometheusPush.NTTCreations.Inc();
                    return ref Entities[arrayIndex];
                }
            }
            throw new IndexOutOfRangeException("Failed to pop an array index");
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref NTT GetEntity(int nttId) => ref Entities[nttId];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref NTT GetEntityByNetId(int netId)
        {
            if (!NetIdToEntityIndex.TryGetValue(netId, out var index))
                return ref Entities[0];

            return ref Entities[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EntityExists(int nttId) => Entities[nttId].Id == nttId;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InformChangesFor(in NTT ntt) => ChangedThisTick.Enqueue(ntt);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Destroy(in NTT ntt) => ToBeRemoved.Enqueue(ntt);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DestroyInternal(in NTT ntt)
        {
            lock (Entities)
            {
                AvailableArrayIndicies.Enqueue(ntt.Id);
                Players.Remove(ntt);
                ntt.Recycle();
                NetIdToEntityIndex.Remove(ntt.NetId);
                ChangedThisTick.Enqueue(ntt);
                Entities[ntt.Id] = default;
            }
            PrometheusPush.NTTCount.Set(EntityCount);
            PrometheusPush.NTTDestroys.Inc();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UpdateNTTs()
        {
            while (ToBeRemoved.TryDequeue(out var ntt))
                DestroyInternal(in ntt);
            SystemNotifier?.Start();
        }
        public static void Update()
        {
            var tickTime = Stopwatch.GetElapsedTime(TickBeginTime);
            TickBeginTime = Stopwatch.GetTimestamp();
            var dt = MathF.Min(1f / TargetTps, (float)tickTime.TotalSeconds);
            TimeAcc += dt;
            UpdateTimeAcc += dt;
            var ts = Stopwatch.GetTimestamp();

            if (UpdateTimeAcc >= UpdateTime)
            {
                UpdateTimeAcc -= UpdateTime;

                for (int i = 0; i < Systems.Length; i++)
                {
                    UpdateNTTs();
                    Systems[i].BeginUpdate();
                }
                UpdateNTTs();
                OnEndTick?.Invoke();
                Tick++;
                PrometheusPush.TickCount.Inc();
            }

            if (TimeAcc >= 1)
            {
                OnSecond?.Invoke();
                TimeAcc = 0;
            }

            var tickDuration = (float)Stopwatch.GetElapsedTime(TickBeginTime).TotalMilliseconds;
            PrometheusPush.TickTime.Observe(tickDuration);
            // PerformanceMetrics.AddSample(nameof(NttWorld), tickDuration);
            var sleepTime = (int)Math.Max(0, -1 + (UpdateTime * 1000) - tickDuration);
            Thread.Sleep(sleepTime);
        }
    }
}