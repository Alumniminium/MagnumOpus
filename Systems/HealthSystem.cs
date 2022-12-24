using MagnumOpus.ECS;
using MagnumOpus.Components;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class HealthSystem : PixelSystem<HealthComponent, HealthRegenComponent>
    {
        public HealthSystem() : base("Health System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref HealthComponent hlt, ref HealthRegenComponent reg)
        {
            
        }
    }
}