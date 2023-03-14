using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct RequestShopItemTransactionComponent
    {
        public int ShopId;
        public int ItemId;
        public bool Buy;

        public RequestShopItemTransactionComponent(int shopId, int itemId, bool buy)
        {
            ShopId = shopId;
            ItemId = itemId;
            Buy = buy;
        }
    }
}