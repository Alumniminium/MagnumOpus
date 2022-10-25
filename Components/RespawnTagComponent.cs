using MagnumOpus.ECS;

namespace MagnumOpus.Simulation.Components
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
            RespawnTimeTick = Game.CurrentTick + Game.TargetTps * respawnTimeDelaySeconds;
            ExpPenalty = expPenalty;
        }
        public override int GetHashCode()
        {
            return EntityId;
        }
    }
}