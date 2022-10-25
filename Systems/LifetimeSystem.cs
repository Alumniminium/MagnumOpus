using MagnumOpus.ECS;
using MagnumOpus.Simulation.Components;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class LifetimeSystem : PixelSystem<LifeTimeComponent>
    {
        public LifetimeSystem() : base("Lifetime System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref LifeTimeComponent c1)
        {
            c1.LifeTimeSeconds -= deltaTime;

            if (c1.LifeTimeSeconds > 0)
                return;

            var dtc = new DeathTagComponent();
            ntt.Add(ref dtc);
        }
    }
}