using MagnumOpus.ECS;
using Newtonsoft.Json;

namespace MagnumOpus.Components.Location
{
    [Component]
    [Save]
    public readonly struct TeleportComponent
    {
        public readonly ushort Map;
        public readonly ushort X;
        public readonly ushort Y;

        [JsonConstructor]
        public TeleportComponent(ushort x, ushort y, ushort map)
        {
            X = x;
            Y = y;
            Map = map;
        }
    }
}