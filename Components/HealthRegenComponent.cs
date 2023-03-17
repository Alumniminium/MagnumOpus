using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct HealthRegenComponent
    {
        public float PassiveHealPerSec;

        public HealthRegenComponent(float healthRegFactor) => PassiveHealPerSec = healthRegFactor;
    }
}