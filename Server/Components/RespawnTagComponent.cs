using NttECS.ECS;
namespace MagnumOpus.Components
{
    [Component(SaveEnabled: true)]
    /// <summary>
    /// Entity respawn delay component that schedules when destroyed entities should be recreated.
    /// Contains respawn tick calculated from delay time for timed entity regeneration. Currently
    /// defined but not actively processed by any systems - represents planned respawn system for
    /// monsters, NPCs, or other entities that should reappear after being killed or destroyed,
    /// enabling persistent world population and resource regeneration.
    /// </summary>
    public struct RespawnTagComponent(int respawnTimeDelaySeconds)
    {
        public long RespawnTimeTick = NttWorld.Tick + (NttWorld.TargetTps * respawnTimeDelaySeconds);
    }
}