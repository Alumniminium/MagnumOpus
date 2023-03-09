using MagnumOpus.ECS;
using Newtonsoft.Json;

namespace MagnumOpus.Components.Floor
{
    [Component]
    [Save]
    public readonly struct PortalComponent
    {
        public readonly ushort X;
        public readonly ushort Y;

        [JsonConstructor]
        public PortalComponent(ushort x, ushort y)
        {
            X = x;
            Y = y;
        }
    }
}