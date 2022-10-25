using MagnumOpus.ECS;

namespace MagnumOpus.Simulation.Components
{
    [Component]
    public readonly struct HealthRegenComponent
    {
        public readonly int EntityId;
        public readonly float PassiveHealPerSec;

        public HealthRegenComponent(int entityId, float healthRegFactor)
        {
            EntityId = entityId;
            PassiveHealPerSec = healthRegFactor;
        }
        public override int GetHashCode()
        {
            return EntityId;
        }
    }
}