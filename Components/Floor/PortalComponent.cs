using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component][Save]
    public readonly struct PortalComponent
    {
        public readonly int EntityId;
        public readonly ushort X;
        public readonly ushort Y;

        public PortalComponent(int id, ushort x, ushort y)
        {
            EntityId = id;
            X = x;
            Y = y;
        }

        public override int GetHashCode() => EntityId;
    }
}