using NttECS.ECS;

namespace MagnumOpus.Components;

[Component]
/// <summary>
/// Transient shop transaction request component that handles both buying and selling items
/// with merchants. Contains shop ID, item ID, and transaction type (buy/sell flag). Not
/// saved to database (no SaveEnabled). Processed by ShopSystem to validate shop availability,
/// handle pricing calculations, manage inventory space, process money transfers, and track
/// economy metrics. Essential for all merchant trading interactions.
/// </summary>
public struct RequestShopItemTransactionComponent(int shopId, int itemId, bool buy)
{
    public int ShopId = shopId;
    public int ItemId = itemId;
    public bool Buy = buy;
}