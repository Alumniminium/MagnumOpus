using MagnumOpus.ECS;
namespace MagnumOpus.Components.Death
{
    [Component]
    [Save]
    public struct RespawnTagComponent
    {
        public long RespawnTimeTick;

        public RespawnTagComponent(int respawnTimeDelaySeconds) => RespawnTimeTick = NttWorld.Tick + (NttWorld.TargetTps * respawnTimeDelaySeconds);
    }
}