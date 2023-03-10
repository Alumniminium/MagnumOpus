using MagnumOpus.ECS;
namespace MagnumOpus.Components.Death
{
    [Component]
    [Save]
    public readonly struct RespawnTagComponent
    {
        public readonly long RespawnTimeTick;

        public RespawnTagComponent(int respawnTimeDelaySeconds) => RespawnTimeTick = NttWorld.Tick + (NttWorld.TargetTps * respawnTimeDelaySeconds);
    }
}