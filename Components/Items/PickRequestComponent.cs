using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public readonly struct PickupRequestComponent
    {
        public readonly int EntityId;
        public readonly NTT Item;

        public PickupRequestComponent(int nttId, in NTT item)
        {
            EntityId = nttId;
            Item = item;
        }

        public override int GetHashCode() => EntityId;
    }
}