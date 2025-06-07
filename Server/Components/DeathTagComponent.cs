using NttECS.ECS;

namespace MagnumOpus.Components
{
    [Component(SaveEnabled: true)]
    /// <summary>
    /// Death marker component that indicates an entity has died and stores death-related data.
    /// Records the killer entity and death timestamp for tracking purposes. Used by multiple 
    /// systems to filter out dead entities from AI targeting, prevent respawn overlaps, and 
    /// manage death-related behaviors. Removed by ReviveSystem when entity is brought back to life.
    /// </summary>
    public struct DeathTagComponent
    {
        public NTT Killer;
        public long Tick;

        public DeathTagComponent() => Tick = NttWorld.Tick;
        public DeathTagComponent(in NTT killer)
        {
            Killer = killer;
            Tick = NttWorld.Tick;
        }
    }
}