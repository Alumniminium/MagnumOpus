using MagnumOpus.ECS;

namespace MagnumOpus.Components.Items
{
    [Component]
    public readonly struct RequestDropMoneyComponent
    {
        public readonly int EntityId;
        public readonly int Amount;

        public RequestDropMoneyComponent(int entityId, int amount)
        {
            EntityId = entityId;
            Amount = amount;
        }

        public override int GetHashCode() => EntityId;
    }
}