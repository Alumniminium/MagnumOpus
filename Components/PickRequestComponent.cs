using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct PickupRequestComponent
    {
        public readonly int EntityId;
        public readonly PixelEntity Item;

        public PickupRequestComponent(int nttId, in PixelEntity item)
        {
            EntityId = nttId;
            Item = item;
        }

        public override int GetHashCode() => EntityId;
    }
}