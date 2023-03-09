using MagnumOpus.ECS;

namespace MagnumOpus.Components.Items
{
    [Component]
    public readonly struct RequestShopItemTransactionComponent
    {
        public readonly int ShopId;
        public readonly int ItemId;
        public readonly bool Buy;

        public RequestShopItemTransactionComponent(int shopId, int itemId, bool buy)
        {
            ShopId = shopId;
            ItemId = itemId;
            Buy = buy;
        }

        
    }
}