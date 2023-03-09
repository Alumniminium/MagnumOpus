using MagnumOpus.ECS;
using Newtonsoft.Json;

namespace MagnumOpus.Components.Death
{
    [Component]
    [Save]
    public readonly struct RespawnTagComponent
    {
        public readonly long RespawnTimeTick;

        [JsonConstructor]
        public RespawnTagComponent(int respawnTimeDelaySeconds) => RespawnTimeTick = NttWorld.Tick + (NttWorld.TargetTps * respawnTimeDelaySeconds);
    }
}