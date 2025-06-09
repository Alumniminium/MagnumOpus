using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Squiggly;
using MagnumOpus.Helpers;
using NttECS.ECS;

namespace MagnumOpus.Systems;

public sealed class ShopSystem : NttSystem<InventoryComponent, RequestShopItemTransactionComponent>
{
    public ShopSystem() : base("Shop", threads: 1, log: false) { }

    // Handles shop transactions including item purchases and sales with inventory management
    // and economy tracking. Validates shop availability, item pricing, manages money transfers
    // with durability-based sell prices, and tracks transaction metrics via Prometheus.
    public override void Update(in NTT ntt, ref InventoryComponent inventory, ref RequestShopItemTransactionComponent transaction)
    {
        // === DETERMINE TARGET ITEM ID ===
        var targetItemId = transaction.ItemId;

        if (!transaction.Buy)
        {
            ref readonly var itemEntity = ref NttWorld.GetEntity(targetItemId);
            ref readonly var itemComponent = ref itemEntity.Get<ItemComponent>();
            targetItemId = itemComponent.Id;
        }

        // === VALIDATE SHOP EXISTENCE ===
        if (!Collections.Shops.TryGetValue(transaction.ShopId, out var shopData))
        {
            FConsole.WriteLine($"[{nameof(ShopSystem)}]: {ntt.Id} tried to {(transaction.Buy ? "buy" : "sell")} from shop {transaction.ShopId} but it doesn't exist in Shops.dat");
            ntt.Remove<RequestShopItemTransactionComponent>();
            return;
        }

        // === VALIDATE ITEM AVAILABILITY IN SHOP ===
        if (!shopData.Items.Contains(targetItemId) && transaction.Buy)
        {
            FConsole.WriteLine($"[{nameof(ShopSystem)}]: {ntt.Id} tried to {(transaction.Buy ? "buy" : "sell")} {targetItemId} but it doesn't exist in the shop {transaction.ShopId}");
            ntt.Remove<RequestShopItemTransactionComponent>();
            return;
        }

        // === VALIDATE ITEM TYPE DATA ===
        if (!Collections.ItemType.TryGetValue(targetItemId, out var itemTypeData))
        {
            FConsole.WriteLine($"[{nameof(ShopSystem)}]: {ntt.Id} tried to {(transaction.Buy ? "buy" : "sell")} {targetItemId} but it doesn't exist in ItemType.dat");
            ntt.Remove<RequestShopItemTransactionComponent>();
            return;
        }

        // === VALIDATE PURCHASE FUNDS ===
        if (inventory.Money < itemTypeData.Price && transaction.Buy)
        {
            FConsole.WriteLine($"[{nameof(ShopSystem)}]: {ntt.Id} tried to buy {targetItemId} with {inventory.Money:C} but it costs {itemTypeData.Price:C}");
            ntt.Remove<RequestShopItemTransactionComponent>();
            return;
        }

        // === PROCESS TRANSACTION ===
        for (var slotIndex = 0; slotIndex < inventory.Items.Length; slotIndex++)
        {
            ref readonly var slotItemComponent = ref inventory.Items.Span[slotIndex].Get<ItemComponent>();

            if ((slotItemComponent.Id != 0 || !transaction.Buy) && (slotItemComponent.Id != targetItemId || transaction.Buy))
                continue;

            if (transaction.Buy)
            {
                // === HANDLE ITEM PURCHASE ===
                inventory.Money -= itemTypeData.Price;
                ref var newItemEntity = ref NttWorld.CreateEntity(IdGenerator.GetItemId());
                var newItemComponent = new ItemComponent(transaction.ItemId, itemTypeData.Amount, itemTypeData.AmountLimit, 0, 0, 0, 0, 0, 0, 0, 0);
                newItemEntity.Set(ref newItemComponent);
                inventory.Items.Span[slotIndex] = newItemEntity;

                var itemInfoMessage = MsgItemInformation.Create(newItemEntity);
                ntt.NetSync(ref itemInfoMessage);

                if (IsLogging)
                    FConsole.WriteLine("{0} bought {1} for {2:C} and now has {3:C}", ntt.Id, transaction.ItemId, itemTypeData.Price, inventory.Money);

                // Track purchase metrics
                PrometheusPush.ServerIncome.Inc(itemTypeData.Price);
                PrometheusPush.ShopIncome.Inc(itemTypeData.Price);
                PrometheusPush.ShopPurchases.Inc();
            }
            else
            {
                // === HANDLE ITEM SALE ===
                Collections.ItemType.TryGetValue(slotItemComponent.Id, out var sellItemInfo);

                // Calculate sell price based on durability (1/3 of base price)
                var sellPrice = sellItemInfo.Price / 3;
                sellPrice = (uint)((double)sellPrice * ((float)slotItemComponent.CurrentDurability / slotItemComponent.MaximumDurability));
                inventory.Money += sellPrice;

                // Remove item from inventory
                ref var soldItemEntity = ref NttWorld.GetEntity(transaction.ItemId);
                var destroyComponent = new DestroyEndOfFrameComponent();
                soldItemEntity.Set(ref destroyComponent);
                inventory.Items.Span[slotIndex] = default;

                var removeInventoryMessage = MsgItem.Create(soldItemEntity.Id, soldItemEntity.Id, MsgItemType.RemoveInventory);
                ntt.NetSync(ref removeInventoryMessage);

                if (IsLogging)
                    FConsole.WriteLine("{0} sold {1} for {2} and now has {3:C}", ntt.Id, transaction.ItemId, sellPrice, inventory.Money);

                // Track sale metrics
                PrometheusPush.ServerExpenses.Inc(sellPrice);
                PrometheusPush.ShopExpenses.Inc(sellPrice);
                PrometheusPush.ShopSales.Inc();
            }
            break;
        }

        ntt.Remove<RequestShopItemTransactionComponent>();
    }
}