using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct RespawnTagComponent(int respawnTimeDelaySeconds)
    {
        public long RespawnTimeTick = NttWorld.Tick + (NttWorld.TargetTps * respawnTimeDelaySeconds);
    }
}