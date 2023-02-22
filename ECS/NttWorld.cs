using System.Diagnostics;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace MagnumOpus.ECS
{
    public static class NttWorld
    {
        public static int TargetTps { get; private set; } = 60;
        private static float UpdateTime => 1f / TargetTps;

        public static readonly NttList NTTs = new();
        private static NttSystem[] Systems;
        public static long Tick { get; private set; }
        private static long TickBeginTime;
        private static float TimeAcc;
        private static float UpdateTimeAcc;

        private static SystemNotifier SystemNotifier;

        private static Action? OnSecond;
        private static Action? OnEndTick;

        static NttWorld()
        {
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            Systems = Array.Empty<NttSystem>();
            PerformanceMetrics.RegisterSystem(nameof(NttWorld));
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
        public static ref NTT CreateEntity(EntityType type) => ref NTTs.CreateEntity(type);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref NTT CreateEntityWithNetId(EntityType type, int netId = 0) => ref NTTs.CreateEntityWithNetId(type, netId);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref NTT GetEntity(int nttId) => ref NTTs.GetEntity(nttId);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref NTT GetEntityByNetId(int netId) => ref NTTs.GetEntityByNetId(netId);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EntityExists(int nttId) => NTTs.EntityExists(nttId);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InformChangesFor(in NTT ntt) => NTTs.InformChangesFor(in ntt);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Destroy(in NTT ntt) => NTTs.Destroy(in ntt);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
                    NTTs.UpdateNTTs();
                    SystemNotifier.Start();
                    Systems[i].BeginUpdate();
                }
                NTTs.UpdateNTTs();
                SystemNotifier.Start();
                OnEndTick?.Invoke();
                Tick++;
                PrometheusPush.TickCount.IncTo(Tick);
            }
            
            if (TimeAcc >= 1)
            {
                OnSecond?.Invoke();
                TimeAcc = 0;
            }

            var tickDuration =  (float)Stopwatch.GetElapsedTime(TickBeginTime).TotalMilliseconds;
            PrometheusPush.TickTime.Set(tickDuration);
            PerformanceMetrics.AddSample(nameof(NttWorld), tickDuration);
            var sleepTime = (int)Math.Max(0, -1 + (UpdateTime * 1000) - tickDuration);
            Thread.Sleep(sleepTime);
        }
    }
}