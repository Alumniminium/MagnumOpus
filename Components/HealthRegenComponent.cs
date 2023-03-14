using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    [Save]
    public struct HealthRegenComponent
    {
        public float PassiveHealPerSec;

        public HealthRegenComponent(float healthRegFactor) => PassiveHealPerSec = healthRegFactor;
    }
}