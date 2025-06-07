using NttECS.ECS;
namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
/// <summary>
/// Entity expiration component that automatically destroys entities after a specified duration.
/// Contains expiration tick calculated from creation time plus lifespan. Used by LifetimeSystem
/// to clean up temporary entities like dropped items, effects, and spawned objects. Also used
/// by PickupSystem to remove lifetime components when items are collected, and DropItemSystem
/// to set expiration times for ground items (typically 5 minutes).
/// </summary>
public struct LifeTimeComponent(TimeSpan timespan)
{
    public uint ExpireTick = (uint)(NttWorld.Tick + NttWorld.TargetTps * timespan.TotalSeconds);
}