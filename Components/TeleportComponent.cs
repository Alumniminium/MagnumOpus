using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component]
    [Save]
    public struct TeleportComponent
    {
        public ushort Map;
        public ushort X;
        public ushort Y;

        public TeleportComponent(ushort x, ushort y, ushort map)
        {
            X = x;
            Y = y;
            Map = map;
        }
    }
}