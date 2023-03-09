using MagnumOpus.ECS;
using Newtonsoft.Json;

namespace MagnumOpus.Components.CQ
{
    [Component]
    [Save]
    public readonly struct RecordPointComponent
    {
        public readonly ushort Map;
        public readonly ushort X;
        public readonly ushort Y;

        [JsonConstructor]
        public RecordPointComponent(ushort x, ushort y, ushort map)
        {
            X = x;
            Y = y;
            Map = map;
        }
    }
}