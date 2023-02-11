using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class LifetimeSystem : NttSystem<LifeTimeComponent>
    {
        public static bool Trace = false;
        public LifetimeSystem() : base("Lifetime", threads: Environment.ProcessorCount) { }

        public override void Update(in NTT ntt, ref LifeTimeComponent ltc)
        {
            if (ltc.ExpireTick > NttWorld.Tick)
            {
                var ticksLeft = ltc.ExpireTick - NttWorld.Tick;
                uint[] countdown = new uint[] { 5, 4, 3, 2, 1 }.Select(sec => (uint)NttWorld.TargetTps * sec).ToArray();

                if (!Array.Exists(countdown, x => x == ticksLeft))
                    return;

                string text = $"{ticksLeft / NttWorld.TargetTps} seconds left";
                if (Trace)
                FConsole.WriteLine($"[{nameof(LifetimeSystem)}] {ntt.Id} -> {text}");
            }

            var dtc = new DeathTagComponent();
            ntt.Set(ref dtc);
            ntt.Remove<LifeTimeComponent>();
                if (Trace)
            FConsole.WriteLine($"[{nameof(LifetimeSystem)}] {ntt.Id} -> EXPIRED");
        }
    }
}