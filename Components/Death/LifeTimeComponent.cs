using MagnumOpus.ECS;
using Newtonsoft.Json;

namespace MagnumOpus.Components.Death
{
    [Component]
    [Save]
    public struct LifeTimeComponent
    {
        public uint ExpireTick;

        [JsonConstructor]
        public LifeTimeComponent(TimeSpan timespan) => ExpireTick = (uint)(NttWorld.Tick + NttWorld.TargetTps * timespan.TotalSeconds);
    }
}