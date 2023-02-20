using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.CompilerServices;
using MagnumOpus.Helpers;
using MagnumOpus.Networking;

namespace MagnumOpus.ECS
{
    public class SystemNotifier
    {
        private readonly NttSystem[] _array;
        private readonly int _threadCount;
        private readonly Thread[] _threads;
        private readonly AutoResetEvent[] _events;
        private readonly AutoResetEvent _readyEvent;
        private int _readyThreads;

        public SystemNotifier(NttSystem[] array)
        {
            _array = array;
            _threadCount = 1;
            _threads = new Thread[_threadCount];
            _events = new AutoResetEvent[_threadCount];
            _readyEvent = new AutoResetEvent(false);
            for (var i = 0; i < _threadCount; i++)
            {
                _events[i] = new AutoResetEvent(false);
                _threads[i] = new Thread(WorkLoop);
                _threads[i].Start(i);
            }
        }

        public void Start()
        {
            Interlocked.Exchange(ref _readyThreads, 0);
            for (var i = 0; i < _threadCount; i++)
                _events[i].Set();
            _readyEvent.WaitOne();
        }

        private void WorkLoop(object idx)
        {
            int id = (int)idx;
            while (true)
            {
                Interlocked.Increment(ref _readyThreads);
                if(_readyThreads == _threadCount)
                    _readyEvent.Set();

                _events[id].WaitOne();

                while (NttWorld.ChangedThisTick.TryDequeue(out var entity))
                {
                    for (int i = 0; i < _array.Length; i++)
                            _array[i].EntityChanged(in entity);
                }

                _readyEvent.Set();
            }
        }
    }
    public static class NttWorld
    {
        public const int MaxEntities = 500_000;
        public static int TargetTps { get; private set; } = 60;
        private static float UpdateTime;

        private static readonly NTT[] Entities;
        private static readonly Dictionary<int, int> NetIdToEntityIndex = new();
        private static readonly ConcurrentQueue<int> AvailableArrayIndicies;
        private static readonly ConcurrentQueue<NTT> ToBeRemoved = new();
        public static readonly HashSet<NTT> Players = new();
        public static readonly ConcurrentQueue<NTT> ChangedThisTick = new();
        private static readonly Stopwatch Stopwatch = new();

        public static int EntityCount => MaxEntities - AvailableArrayIndicies.Count;
        private static NttSystem[] Systems;
        public static long Tick { get; private set; }
        private static float TimeAcc;
        private static float UpdateTimeAcc;

        private static SystemNotifier SystemNotifier;

        private static Action? OnSecond;
        private static Action? OnTick;

        static NttWorld()
        {
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            Entities = new NTT[MaxEntities];
            AvailableArrayIndicies = new(Enumerable.Range(1, MaxEntities - 1));
            Systems = Array.Empty<NttSystem>();
            PerformanceMetrics.RegisterSystem("SLEEP");
            PerformanceMetrics.RegisterSystem(nameof(NttWorld));
        }

        public static void SetSystems(params NttSystem[] systems)
        {
            Systems = systems;
            SystemNotifier = new SystemNotifier(systems);
        }

        public static void SetTPS(int fps)
        {
            TargetTps = fps;
            UpdateTime = 1f / TargetTps;
        }

        public static void RegisterOnSecond(Action action) => OnSecond += action;
        public static void RegisterOnTick(Action action) => OnTick += action;

        public static ref NTT CreateEntity(EntityType type)
        {
            lock (Entities)
            {
                if (AvailableArrayIndicies.TryDequeue(out var arrayIndex))
                {
                    var netId = IdGenerator.Get(type);
                    Entities[arrayIndex] = new NTT(arrayIndex, netId, type);
                    NetIdToEntityIndex.Add(netId, arrayIndex);
                    return ref Entities[arrayIndex];
                }
            }
            throw new IndexOutOfRangeException("Failed to pop an array index");
        }
        public static ref NTT CreateEntityWithNetId(EntityType type, int netId = 0)
        {
            lock (Entities)
            {
                if (AvailableArrayIndicies.TryDequeue(out var arrayIndex))
                {
                    Entities[arrayIndex] = new NTT(arrayIndex, netId, type);
                    NetIdToEntityIndex.Add(netId, arrayIndex);
                    return ref Entities[arrayIndex];
                }
            }
            throw new IndexOutOfRangeException("Failed to pop an array index");
        }
        public static ref NTT GetEntity(int nttId) => ref Entities[nttId];

        public static ref NTT GetEntityByNetId(int netId)
        {
            if (!NetIdToEntityIndex.TryGetValue(netId, out var index))
                return ref Entities[0];

            return ref Entities[index];
        }

        public static bool EntityExists(int nttId) => Entities[nttId].Id == nttId;
        public static void InformChangesFor(in NTT ntt) => ChangedThisTick.Enqueue(ntt);
        public static void Destroy(in NTT ntt) => ToBeRemoved.Enqueue(ntt);

        private static void DestroyInternal(in NTT ntt)
        {
            lock (Entities)
            {
                AvailableArrayIndicies.Enqueue(ntt.Id);
                Players.Remove(ntt);
                ntt.Recycle();
                NetIdToEntityIndex.Remove(ntt.NetId);
                Entities[ntt.Id] = default;
                ChangedThisTick.Enqueue(ntt);
            }
        }
        public static void Update()
        {
            var dt = MathF.Min(1f / TargetTps, (float)Stopwatch.Elapsed.TotalSeconds);
            TimeAcc += dt;
            UpdateTimeAcc += dt;
            Stopwatch.Restart();
            double last = Stopwatch.Elapsed.TotalMilliseconds;

            if (UpdateTimeAcc >= UpdateTime)
            {
                UpdateTimeAcc -= UpdateTime;

                for (int i = 0; i < Systems.Length; i++)
                {
                    UpdateNTTs();
                    Systems[i].BeginUpdate();
                }
                UpdateNTTs();

                if (TimeAcc >= 1)
                {
                    OnSecond?.Invoke();
                    TimeAcc = 0;
                }
                OnTick?.Invoke();
                Tick++;
            }

            last = Stopwatch.Elapsed.TotalMilliseconds;
            PerformanceMetrics.AddSample(nameof(NttWorld), Stopwatch.Elapsed.TotalMilliseconds);

            last = Stopwatch.Elapsed.TotalMilliseconds;
            var sleepTime = (int)Math.Max(0, -1 + (UpdateTime * 1000) - last);
            Thread.Sleep(sleepTime);
            PerformanceMetrics.AddSample("SLEEP", Stopwatch.Elapsed.TotalMilliseconds - last);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DestroyNTTs()
        {
            while (ToBeRemoved.TryDequeue(out var ntt))
                DestroyInternal(ntt);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void UpdateNTTs()
        {
            DestroyNTTs();
            SystemNotifier.Start();
        }
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // private static void UpdateNTTs()
        // {
        //     for(int i = 0; i < Systems.Length; i++)
        //         if(ChangedThisTick.TryDequeue(out var ntt))
        //             Systems[i].EntityChanged(in ntt);
        // }
    }
}