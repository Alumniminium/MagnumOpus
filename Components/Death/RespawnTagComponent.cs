using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component][Save]
    public readonly struct RespawnTagComponent
    {
        public readonly int EntityId;
        public readonly long RespawnTimeTick;

        public RespawnTagComponent(int entityId, int respawnTimeDelaySeconds)
        {
            EntityId = entityId;
            RespawnTimeTick = NttWorld.Tick + NttWorld.TargetTps * respawnTimeDelaySeconds;
        }
        
        public override int GetHashCode() => EntityId;
    }
}