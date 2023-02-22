using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component][Save]
    public struct VirtuePointComponent
    {
        public readonly int EntityId;
        public long Points;

        public VirtuePointComponent(int entityId, long points)
        {
            EntityId = entityId;
            Points = points;
        }

        public override int GetHashCode() => EntityId;
    }
}