using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public readonly struct RespawnTagComponent
    {
        public readonly int EntityId;
        public readonly uint ExpPenalty;
        public readonly long RespawnTimeTick;

        public RespawnTagComponent(int entityId, uint expPenalty, int respawnTimeDelaySeconds)
        {
            EntityId = entityId;
            RespawnTimeTick = NttWorld.Tick + NttWorld.TargetTps * respawnTimeDelaySeconds;
            ExpPenalty = expPenalty;
        }
        
        public override int GetHashCode() => EntityId;
    }
}