using System.Diagnostics;
using System.Runtime;
using MagnumOpus.Networking;

namespace MagnumOpus.ECS
{
    public static class PixelWorld
    {
        public const int MaxEntities = 1500_000;
        public static int TargetTps { get; private set; } = 30;
        private static float UpdateTime;

        private static readonly PixelEntity[] Entities;
        private static readonly Stack<int> AvailableArrayIndicies;
        private static readonly Stack<PixelEntity> ToBeRemoved = new();
        public static readonly List<PixelEntity> Players = new();
        public static readonly HashSet<PixelEntity> ChangedThisTick = new();
        private static readonly Stopwatch Stopwatch = new();

        public static int EntityCount => MaxEntities - AvailableArrayIndicies.Count;
        private static PixelSystem[] Systems;
        public static uint Tick { get; private set; }
        private static float TimeAcc;
        private static float UpdateTimeAcc;

        private static Action? OnSecond;
        private static Action? OnTick;

        static PixelWorld()
        {
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            Entities = new PixelEntity[MaxEntities];
            AvailableArrayIndicies = new(Enumerable.Range(1, MaxEntities - 1));
            Systems = Array.Empty<PixelSystem>();
            PerformanceMetrics.RegisterSystem(nameof(IncomingPacketQueue));
            PerformanceMetrics.RegisterSystem(nameof(OutgoingPacketQueue));
            PerformanceMetrics.RegisterSystem("SLEEP");
            PerformanceMetrics.RegisterSystem(nameof(PixelWorld));
        }

        public static void SetSystems(params PixelSystem[] systems) => Systems = systems;
        public static void SetFPS(int fps)
        {
            TargetTps = fps;
            UpdateTime = 1f / TargetTps;
        }

        public static void RegisterOnSecond(Action action) => OnSecond += action;
        public static void RegisterOnTick(Action action) => OnTick += action;

        public static ref PixelEntity CreateEntity(EntityType type, int parentId = -1)
        {
            if (AvailableArrayIndicies.TryPop(out var arrayIndex))
            {
                Entities[arrayIndex] = new PixelEntity(arrayIndex, type, parentId);
                return ref Entities[arrayIndex];
            }
            throw new IndexOutOfRangeException("Failed to pop an array index");
        }
        public static ref PixelEntity GetEntity(int nttId) => ref Entities[nttId];

        public static bool EntityExists(int nttId) => Entities[nttId].Id == nttId;

        public static bool EntityExists(in PixelEntity ntt) => Entities[ntt.Id].Id == ntt.Id;

        public static void InformChangesFor(in PixelEntity ntt) => ChangedThisTick.Add(ntt);

        public static void Destroy(in PixelEntity ntt) => ToBeRemoved.Push(ntt);

        private static void DestroyInternal(in PixelEntity ntt)
        {
            foreach (var child in ntt.Children)
                DestroyInternal(child);

            AvailableArrayIndicies.Push(ntt.Id);
            Players.Remove(ntt);
            OutgoingPacketQueue.Remove(in ntt);
            IncomingPacketQueue.Remove(in ntt);
            ntt.Recycle();
            Entities[ntt.Id] = default;

            for (int i = 0; i < Systems.Length; i++)
                Systems[i].EntityChanged(in ntt);
        }
        public static void Update()
        {
            var dt = MathF.Min(1f / TargetTps, (float)Stopwatch.Elapsed.TotalSeconds);
            TimeAcc += dt;
            UpdateTimeAcc += dt;
            Stopwatch.Restart();
            double last = Stopwatch.Elapsed.TotalMilliseconds;

            IncomingPacketQueue.ProcessAll();
            PerformanceMetrics.AddSample(nameof(IncomingPacketQueue), Stopwatch.Elapsed.TotalMilliseconds - last);

            if (UpdateTimeAcc >= UpdateTime)
            {
                UpdateTimeAcc -= UpdateTime;
                for (int i = 0; i < Systems.Length; i++)
                {
                    last = Stopwatch.Elapsed.TotalMilliseconds;
                    var system = Systems[i];
                    system.Update(dt);
                    while (ToBeRemoved.Count != 0)
                        DestroyInternal(ToBeRemoved.Pop());
                    foreach (var ntt in ChangedThisTick)
                    {
                        for (int x = 0; x < Systems.Length; x++)
                            Systems[x].EntityChanged(in ntt);
                    }
                    ChangedThisTick.Clear();
                    PerformanceMetrics.AddSample(system.Name, Stopwatch.Elapsed.TotalMilliseconds - last);
                    last = Stopwatch.Elapsed.TotalMilliseconds;

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
            OutgoingPacketQueue.SendAll();
            PerformanceMetrics.AddSample(nameof(OutgoingPacketQueue), Stopwatch.Elapsed.TotalMilliseconds - last);
            PerformanceMetrics.AddSample(nameof(PixelWorld), Stopwatch.Elapsed.TotalMilliseconds);

            last = Stopwatch.Elapsed.TotalMilliseconds;
            var sleepTime = (int)Math.Max(0, -1 + (UpdateTime * 1000) - last);
            Thread.Sleep(sleepTime);
            PerformanceMetrics.AddSample("SLEEP", Stopwatch.Elapsed.TotalMilliseconds - last);
        }
    }
}