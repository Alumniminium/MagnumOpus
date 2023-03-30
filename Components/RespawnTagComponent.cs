using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct RespawnTagComponent
    {
        public long RespawnTimeTick;

        public RespawnTagComponent(int respawnTimeDelaySeconds) => RespawnTimeTick = NttWorld.Tick + (NttWorld.TargetTps * respawnTimeDelaySeconds);
    }
}