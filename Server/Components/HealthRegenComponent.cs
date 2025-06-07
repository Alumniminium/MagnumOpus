using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
/// <summary>
/// Passive health regeneration component that enables gradual health recovery over time.
/// Contains the regeneration rate (health per second) for automatic healing. Currently
/// defined but not actively processed by any systems - represents planned regeneration
/// functionality for entities that should slowly heal outside of combat scenarios.
/// </summary>
public struct HealthRegenComponent(float healthRegFactor)
{
    public float PassiveHealPerSec = healthRegFactor;
}