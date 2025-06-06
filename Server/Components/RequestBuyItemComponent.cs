using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct RequestShopItemTransactionComponent(int shopId, int itemId, bool buy)
    {
        public int ShopId = shopId;
        public int ItemId = itemId;
        public bool Buy = buy;
    }
}