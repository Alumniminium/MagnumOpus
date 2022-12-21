using MagnumOpus.ECS;
using MagnumOpus.Components;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class LifetimeSystem : PixelSystem<LifeTimeComponent>
    {
        public LifetimeSystem() : base("Lifetime System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref LifeTimeComponent ltc)
        {
            ltc.LifeTimeSeconds -= deltaTime;

            if (ltc.LifeTimeSeconds > 0)
                return;

            var dtc = new DeathTagComponent();
            ntt.Add(ref dtc);
        }
    }
}