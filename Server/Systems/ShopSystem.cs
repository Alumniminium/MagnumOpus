using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Squiggly;
using MagnumOpus.Helpers;
using NttECS.ECS;

namespace MagnumOpus.Systems
{
    /// <summary>
    /// Handles shop transactions including item purchases and sales with inventory management and economy tracking.
    /// Validates shop availability, item pricing, and manages money transfers with durability-based sell prices.
    /// </summary>
    public sealed class ShopSystem : NttSystem<InventoryComponent, RequestShopItemTransactionComponent>
    {
        /// <summary>
        /// Initializes the ShopSystem with limited threading for transaction processing.
        /// </summary>
        public ShopSystem() : base("Shop", threads: 1, log: false) { }

        /// <summary>
        /// Processes shop transactions, handling both item purchases and sales with appropriate validation and economy tracking.
        /// </summary>
        /// <param name="ntt">The entity making the transaction</param>
        /// <param name="inventory">Inventory component for item and money management</param>
        /// <param name="transaction">Transaction component specifying the shop operation</param>
        public override void Update(in NTT ntt, ref InventoryComponent inventory, ref RequestShopItemTransactionComponent transaction)
        {
            var targetItemId = transaction.ItemId;

            if (!transaction.Buy)
            {
                ref readonly var itemEntity = ref NttWorld.GetEntity(targetItemId);
                ref readonly var itemComponent = ref itemEntity.Get<ItemComponent>();
                targetItemId = itemComponent.Id;
            }

            if (!Collections.Shops.TryGetValue(transaction.ShopId, out var shopData))
            {
                FConsole.WriteLine($"[{nameof(ShopSystem)}]: {ntt.Id} tried to {(transaction.Buy ? "buy" : "sell")} from shop {transaction.ShopId} but it doesn't exist in Shops.dat");
                ntt.Remove<RequestShopItemTransactionComponent>();
                return;
            }

            if (!shopData.Items.Contains(targetItemId) && transaction.Buy)
            {
                FConsole.WriteLine($"[{nameof(ShopSystem)}]: {ntt.Id} tried to {(transaction.Buy ? "buy" : "sell")} {targetItemId} but it doesn't exist in the shop {transaction.ShopId}");
                ntt.Remove<RequestShopItemTransactionComponent>();
                return;
            }

            if (!Collections.ItemType.TryGetValue(targetItemId, out var itemTypeData))
            {
                FConsole.WriteLine($"[{nameof(ShopSystem)}]: {ntt.Id} tried to {(transaction.Buy ? "buy" : "sell")} {targetItemId} but it doesn't exist in ItemType.dat");
                ntt.Remove<RequestShopItemTransactionComponent>();
                return;
            }

            if (inventory.Money < itemTypeData.Price && transaction.Buy)
            {
                FConsole.WriteLine($"[{nameof(ShopSystem)}]: {ntt.Id} tried to buy {targetItemId} with {inventory.Money:C} but it costs {itemTypeData.Price:C}");
                ntt.Remove<RequestShopItemTransactionComponent>();
                return;
            }


            for (var slotIndex = 0; slotIndex < inventory.Items.Length; slotIndex++)
            {
                ref readonly var slotItemComponent = ref inventory.Items.Span[slotIndex].Get<ItemComponent>();
                if ((slotItemComponent.Id == 0 && transaction.Buy) || (slotItemComponent.Id == targetItemId && !transaction.Buy))
                {
                    if (transaction.Buy)
                    {
                        inventory.Money -= itemTypeData.Price;
                        ref var newItemEntity = ref NttWorld.CreateEntity(IdGenerator.GetItemId());
                        var newItemComponent = new ItemComponent(transaction.ItemId, itemTypeData.Amount, itemTypeData.AmountLimit, 0, 0, 0, 0, 0, 0, 0, 0);
                        newItemEntity.Set(ref newItemComponent);
                        inventory.Items.Span[slotIndex] = newItemEntity;

                        var itemInfoMessage = MsgItemInformation.Create(newItemEntity);
                        ntt.NetSync(ref itemInfoMessage);

                        if (IsLogging)
                            FConsole.WriteLine("{0} bought {1} for {2:C} and now has {3:C}", ntt.Id, transaction.ItemId, itemTypeData.Price, inventory.Money);
                        PrometheusPush.ServerIncome.Inc(itemTypeData.Price);
                        PrometheusPush.ShopIncome.Inc(itemTypeData.Price);
                        PrometheusPush.ShopPurchases.Inc();
                    }
                    else
                    {
                        Collections.ItemType.TryGetValue(slotItemComponent.Id, out var sellItemInfo);

                        var sellPrice = sellItemInfo.Price / 3;
                        sellPrice = (uint)((double)sellPrice * ((float)slotItemComponent.CurrentDurability / slotItemComponent.MaximumDurability));
                        inventory.Money += sellPrice;

                        ref var soldItemEntity = ref NttWorld.GetEntity(transaction.ItemId);
                        var destroyComponent = new DestroyEndOfFrameComponent();
                        soldItemEntity.Set(ref destroyComponent);

                        inventory.Items.Span[slotIndex] = default;

                        var removeInventoryMessage = MsgItem.Create(soldItemEntity.Id, soldItemEntity.Id, soldItemEntity.Id, MsgItemType.RemoveInventory);
                        ntt.NetSync(ref removeInventoryMessage);
                        if (IsLogging)
                            FConsole.WriteLine("{0} sold {1} for {2} and now has {3:C}", ntt.Id, transaction.ItemId, sellPrice, inventory.Money);

                        PrometheusPush.ServerExpenses.Inc(sellPrice);
                        PrometheusPush.ShopExpenses.Inc(sellPrice);
                        PrometheusPush.ShopSales.Inc();
                    }
                    break;
                }
            }

            ntt.Remove<RequestShopItemTransactionComponent>();
        }
    }
}