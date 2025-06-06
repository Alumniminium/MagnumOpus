using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct HealthRegenComponent(float healthRegFactor)
    {
        public float PassiveHealPerSec = healthRegFactor;
    }
}