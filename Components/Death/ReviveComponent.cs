using MagnumOpus.ECS;
using Newtonsoft.Json;

namespace MagnumOpus.Components.Death
{
    [Component]
    [Save]
    public readonly struct ReviveComponent
    {
        public readonly long ReviveTick;
        [JsonConstructor]
        public ReviveComponent(uint seconds) => ReviveTick = NttWorld.Tick + seconds * NttWorld.TargetTps;
    }
}