using System.Diagnostics;
using System.Runtime;
using MagnumOpus.Helpers;
using MagnumOpus.Networking;

namespace MagnumOpus.ECS
{
    public static class NttWorld
    {
        public const int MaxEntities = 1_500_000;
        public static int TargetTps { get; private set; } = 60;
        private static float UpdateTime;

        private static readonly NTT[] Entities;
        private static readonly Dictionary<int, int> NetIdToEntityIndex = new();
        private static readonly Stack<int> AvailableArrayIndicies;
        private static readonly HashSet<NTT> ToBeRemoved = new();
        public static readonly List<NTT> Players = new();
        public static readonly HashSet<NTT> ChangedThisTick = new();
        private static readonly Stopwatch Stopwatch = new();

        public static int EntityCount => MaxEntities - AvailableArrayIndicies.Count;
        private static NttSystem[] Systems;
        public static long Tick { get; private set; }
        private static float TimeAcc;
        private static float UpdateTimeAcc;

        private static Action? OnSecond;
        private static Action? OnTick;

        static NttWorld()
        {
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            Entities = new NTT[MaxEntities];
            AvailableArrayIndicies = new(Enumerable.Range(1, MaxEntities - 1));
            Systems = Array.Empty<NttSystem>();
            PerformanceMetrics.RegisterSystem(nameof(PacketsIn));
            PerformanceMetrics.RegisterSystem(nameof(PacketsOut));
            PerformanceMetrics.RegisterSystem("SLEEP");
            PerformanceMetrics.RegisterSystem(nameof(NttWorld));
        }

        public static void SetSystems(params NttSystem[] systems) => Systems = systems;
        public static void SetTPS(int fps)
        {
            TargetTps = fps;
            UpdateTime = 1f / TargetTps;
        }

        public static void RegisterOnSecond(Action action) => OnSecond += action;
        public static void RegisterOnTick(Action action) => OnTick += action;

        public static ref NTT CreateEntity(EntityType type)
        {
            if (AvailableArrayIndicies.TryPop(out var arrayIndex))
            {
                var netId = IdGenerator.Get(type);
                Entities[arrayIndex] = new NTT(arrayIndex, netId, type);
                NetIdToEntityIndex.Add(netId, arrayIndex);
                return ref Entities[arrayIndex];
            }
            throw new IndexOutOfRangeException("Failed to pop an array index");
        }
        public static ref NTT CreateEntityWithNetId(EntityType type, int netId = 0)
        {
            if (AvailableArrayIndicies.TryPop(out var arrayIndex))
            {
                Entities[arrayIndex] = new NTT(arrayIndex, netId, type);
                NetIdToEntityIndex.Add(netId, arrayIndex);
                return ref Entities[arrayIndex];
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

        public static void InformChangesFor(in NTT ntt)
        {
            lock (ChangedThisTick)
                ChangedThisTick.Add(ntt);
        }

        public static void Destroy(in NTT ntt) => ToBeRemoved.Add(ntt);

        private static void DestroyInternal(in NTT ntt)
        {
            if (ChangedThisTick.Contains(ntt))
                ChangedThisTick.Remove(ntt);

            AvailableArrayIndicies.Push(ntt.Id);
            Players.Remove(ntt);
            PacketsOut.Remove(in ntt);
            PacketsIn.Remove(in ntt);
            ntt.Recycle();
            NetIdToEntityIndex.Remove(ntt.NetId);
            Entities[ntt.Id] = default;

            for (int i = 0; i < Systems.Length; i++)
                Systems[i].EntityChanged(in ntt);
        }
        // public static void Update()
        // {
        //     var dt = MathF.Min(1f / TargetTps, (float)Stopwatch.Elapsed.TotalSeconds);
        //     TimeAcc += dt;
        //     UpdateTimeAcc += dt;
        //     Stopwatch.Restart();
        //     double last = Stopwatch.Elapsed.TotalMilliseconds;

        //     PerformanceMetrics.AddSample(nameof(PacketsIn), Stopwatch.Elapsed.TotalMilliseconds - last);

        //     if (UpdateTimeAcc >= UpdateTime)
        //     {
        //         UpdateTimeAcc -= UpdateTime;

        //         PacketsIn.ProcessAll();

        //         for (int i = 0; i < Systems.Length; i++)
        //         {
        //             lock (ChangedThisTick)
        //             {
        //                 foreach (var ntt in ChangedThisTick)
        //                 {
        //                     for (int x = 0; x < Systems.Length; x++)
        //                         Systems[x].EntityChanged(in ntt);
        //                 }
        //                 ChangedThisTick.Clear();
        //             }
        //             var system = Systems[i];
        //             system.BeginUpdate();

        //         }

        //         while (ToBeRemoved.Count != 0)
        //             DestroyInternal(ToBeRemoved.Pop());

        //         if (TimeAcc >= 1)
        //         {
        //             OnSecond?.Invoke();
        //             TimeAcc = 0;
        //         }
        //         OnTick?.Invoke();
        //         Tick++;
        //     }

        //     last = Stopwatch.Elapsed.TotalMilliseconds;
        //     PacketsOut.SendAll();
        //     PerformanceMetrics.AddSample(nameof(PacketsOut), Stopwatch.Elapsed.TotalMilliseconds - last);
        //     PerformanceMetrics.AddSample(nameof(NttWorld), Stopwatch.Elapsed.TotalMilliseconds);

        //     last = Stopwatch.Elapsed.TotalMilliseconds;
        //     var sleepTime = (int)Math.Max(0, -1 + (UpdateTime * 1000) - last);
        //     Thread.Sleep(sleepTime);
        //     PerformanceMetrics.AddSample("SLEEP", Stopwatch.Elapsed.TotalMilliseconds - last);
        // }
        public static void Update()
        {
            var dt = MathF.Min(1f / TargetTps, (float)Stopwatch.Elapsed.TotalSeconds);
            TimeAcc += dt;
            UpdateTimeAcc += dt;
            Stopwatch.Restart();
            double last = Stopwatch.Elapsed.TotalMilliseconds;

            PerformanceMetrics.AddSample(nameof(PacketsIn), Stopwatch.Elapsed.TotalMilliseconds - last);

            if (UpdateTimeAcc >= UpdateTime)
            {
                UpdateTimeAcc -= UpdateTime;

                foreach (var ntt in ToBeRemoved)
                    DestroyInternal(ntt);
                ToBeRemoved.Clear();

                PacketsIn.ProcessAll();

                for (int i = 0; i < Systems.Length; i++)
                {
                    lock (ChangedThisTick)
                    {
                        foreach (var ntt in ChangedThisTick)
                        {
                            for (int x = 0; x < Systems.Length; x++)
                                Systems[x].EntityChanged(in ntt);
                        }
                        ChangedThisTick.Clear();
                    }

                    var system = Systems[i];
                    system.BeginUpdate();
                }
                lock (ChangedThisTick)
                {
                    foreach (var ntt in ChangedThisTick)
                    {
                        for (int x = 0; x < Systems.Length; x++)
                            Systems[x].EntityChanged(in ntt);
                    }
                    ChangedThisTick.Clear();
                }

                if (TimeAcc >= 1)
                {
                    OnSecond?.Invoke();
                    TimeAcc = 0;
                }
                OnTick?.Invoke();
                Tick++;
            }

            last = Stopwatch.Elapsed.TotalMilliseconds;
            PacketsOut.SendAll();
            PerformanceMetrics.AddSample(nameof(PacketsOut), Stopwatch.Elapsed.TotalMilliseconds - last);
            PerformanceMetrics.AddSample(nameof(NttWorld), Stopwatch.Elapsed.TotalMilliseconds);

            last = Stopwatch.Elapsed.TotalMilliseconds;
            var sleepTime = (int)Math.Max(0, -1 + (UpdateTime * 1000) - last);
            Thread.Sleep(sleepTime);
            PerformanceMetrics.AddSample("SLEEP", Stopwatch.Elapsed.TotalMilliseconds - last);
        }
    }
}