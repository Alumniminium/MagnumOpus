using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public readonly struct MoneyRewardComponent
    {
        public readonly int EntityId;
        public readonly int Amount;

        public MoneyRewardComponent(int entityId, int amount)
        {
            EntityId = entityId;
            Amount = amount;
        }

        public override int GetHashCode() => EntityId;
    }
}