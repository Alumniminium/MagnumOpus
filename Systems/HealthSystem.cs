using MagnumOpus.ECS;
using MagnumOpus.Simulation.Components;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class HealthSystem : PixelSystem<HealthComponent, HealthRegenComponent>
    {
        public HealthSystem() : base("Health System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref HealthComponent hlt, ref HealthRegenComponent reg)
        {
            if (hlt.Health == hlt.MaxHealth)
                return;
            if (ntt.Has<RespawnTagComponent>())
                return;

            var lastHealth = hlt.Health;
            hlt.Health += (ushort)(reg.PassiveHealPerSec * deltaTime);

            if (hlt.Health > hlt.MaxHealth)
                hlt.Health = hlt.MaxHealth;

            if (lastHealth != hlt.Health)
                hlt.ChangedTick = ConquerWorld.Tick;
        }
    }
}