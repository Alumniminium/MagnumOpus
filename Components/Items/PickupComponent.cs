using MagnumOpus.ECS;

namespace MagnumOpus.Components.Items
{
    [Component]
    public readonly struct RequestDropItemComponent
    {
        public readonly int EntityId;
        public readonly NTT ItemNtt;

        public RequestDropItemComponent(int entityId, in NTT itemNtt)
        {
            EntityId = entityId;
            ItemNtt = itemNtt;
        }

        public override int GetHashCode() => EntityId;
    }
}