using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct PortalComponent(ushort x, ushort y)
    {
        public ushort X = x;
        public ushort Y = y;
    }
}