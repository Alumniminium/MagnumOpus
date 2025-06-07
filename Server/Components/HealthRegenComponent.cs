using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component(SaveEnabled: true)]
    public struct HealthRegenComponent(float healthRegFactor)
    {
        public float PassiveHealPerSec = healthRegFactor;
    }
}