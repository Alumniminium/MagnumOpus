using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class LifetimeSystem : PixelSystem<LifeTimeComponent>
    {
        public LifetimeSystem() : base("Lifetime System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref LifeTimeComponent ltc)
        {
            if (ltc.ExpireTick > PixelWorld.Tick)
            {
                var ticksLeft = ltc.ExpireTick - PixelWorld.Tick;
                uint[] countdown = new uint[] { 5, 4, 3, 2, 1 }.Select(sec => (uint)PixelWorld.TargetTps * sec).ToArray();

                if (!Array.Exists(countdown, x => x == ticksLeft))
                    return;

                string text = $"{ticksLeft / PixelWorld.TargetTps} seconds left";
                FConsole.WriteLine($"[{nameof(LifetimeSystem)}] {ntt.Id} -> {text}");
            }

            var dtc = new DeathTagComponent();
            ntt.Set(ref dtc);
            ntt.Remove<LifeTimeComponent>();
            FConsole.WriteLine($"[{nameof(LifetimeSystem)}] {ntt.Id} -> EXPIRED");
        }
    }
}