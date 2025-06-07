using NttECS.ECS;
namespace MagnumOpus.Components
{
    [Component(SaveEnabled: true)]
    /// <summary>
    /// Player revival scheduling component that manages automatic resurrection after death.
    /// Contains revival tick calculated from delay time to control when revival occurs.
    /// Used by ReviveSystem to restore dead players to life, teleport to respawn locations,
    /// restore health, clear death status effects, and handle revival network synchronization.
    /// Essential for death recovery mechanics and player respawn functionality.
    /// </summary>
    public struct ReviveComponent(uint seconds)
    {
        public long ReviveTick = NttWorld.Tick + (seconds * NttWorld.TargetTps);
    }
}