using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public readonly struct RequestDropItemComponent
    {
        public readonly int EntityId;
        public readonly PixelEntity ItemNtt;

        public RequestDropItemComponent(int entityId, in PixelEntity itemNtt)
        {
            EntityId = entityId;
            ItemNtt = itemNtt;
        }

        public override int GetHashCode() => EntityId;
    }
}