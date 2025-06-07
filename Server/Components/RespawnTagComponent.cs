using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component(SaveEnabled: true)]
    public struct RespawnTagComponent(int respawnTimeDelaySeconds)
    {
        public long RespawnTimeTick = NttWorld.Tick + (NttWorld.TargetTps * respawnTimeDelaySeconds);
    }
}