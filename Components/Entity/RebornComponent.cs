using MagnumOpus.ECS;
using Newtonsoft.Json;

namespace MagnumOpus.Components.Entity
{
    [Component]
    [Save]
    public struct RebornComponent
    {
        public long ChangedTick;

        public byte Count;

        [JsonConstructor]
        public RebornComponent()
        {
            ChangedTick = NttWorld.Tick;
            Count = 0;
        }
    }
}