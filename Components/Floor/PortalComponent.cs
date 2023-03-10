using MagnumOpus.ECS;
namespace MagnumOpus.Components.Floor
{
    [Component]
    [Save]
    public readonly struct PortalComponent
    {
        public readonly ushort X;
        public readonly ushort Y;

        public PortalComponent(ushort x, ushort y)
        {
            X = x;
            Y = y;
        }
    }
}