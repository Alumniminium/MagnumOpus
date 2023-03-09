using MagnumOpus.ECS;

namespace MagnumOpus.Components.Location
{
    [Component]
    [Save]
    public readonly struct TeleportComponent
    {
        public readonly int EntityId;
        public readonly ushort Map;
        public readonly ushort X;
        public readonly ushort Y;

        public TeleportComponent(int id, ushort x, ushort y, ushort map)
        {
            EntityId = id;
            X = x;
            Y = y;
            Map = map;
        }

        public override int GetHashCode() => EntityId;
    }
}