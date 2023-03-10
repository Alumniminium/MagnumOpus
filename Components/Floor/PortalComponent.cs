using MagnumOpus.ECS;
namespace MagnumOpus.Components.Floor
{
    [Component]
    [Save]
    public  struct PortalComponent
    {
        public  ushort X;
        public  ushort Y;

        public PortalComponent(ushort x, ushort y)
        {
            X = x;
            Y = y;
        }
    }
}