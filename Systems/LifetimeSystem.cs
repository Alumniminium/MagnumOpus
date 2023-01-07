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
                return;
            
            var dtc = new DeathTagComponent();
            ntt.Add(ref dtc);
            ntt.Remove<LifeTimeComponent>();
        }
    }
}