using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component][Save]
    public readonly struct DeathTagComponent
    {
        public readonly int EntityId;
        public readonly NTT Killer;
        public readonly long Tick;
        
        public DeathTagComponent(int entityId, in NTT killer)
        {
            EntityId = entityId;
            Killer = killer;
            Tick = NttWorld.Tick;
        }
        public override int GetHashCode() => EntityId;
    }
}