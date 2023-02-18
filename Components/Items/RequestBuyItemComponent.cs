using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public readonly struct RequestShopItemTransactionComponent
    {
        public readonly int EntityId;
        public readonly int ShopId;
        public readonly int ItemId;
        public readonly bool Buy;

        public RequestShopItemTransactionComponent(int entityId, int shopId, int itemId, bool buy)
        {
            EntityId = entityId;
            ShopId = shopId;
            ItemId = itemId;
            Buy = buy;
        }
        
        public override int GetHashCode() => EntityId;
    }
}