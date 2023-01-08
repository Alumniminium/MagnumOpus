using MagnumOpus.ECS;
using MagnumOpus.Squiggly;

namespace MagnumOpus.Components
{
    [Component]
    public struct DropComponent
    {
        public readonly int EntityId;
        public readonly Drops Drops;

        public DropComponent(int nttId, Drops drops)
        {
            EntityId = nttId;
            Drops = drops;
        }
        public override int GetHashCode() => EntityId;
    }
}