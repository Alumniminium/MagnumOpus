using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    [Save]
    public readonly struct HealthRegenComponent
    {
        public readonly float PassiveHealPerSec;

        public HealthRegenComponent(float healthRegFactor) => PassiveHealPerSec = healthRegFactor;
    }
}