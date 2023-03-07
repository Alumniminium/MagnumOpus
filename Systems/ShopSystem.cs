using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Squiggly;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class ShopSystem : NttSystem<InventoryComponent, RequestShopItemTransactionComponent>
    {
        public ShopSystem() : base("Shop", threads: 2) { }

        public override void Update(in NTT ntt, ref InventoryComponent inv, ref RequestShopItemTransactionComponent txc)
        {
            var itemId = txc.ItemId;

            if (!txc.Buy)
            {
                ref readonly var itemNtt = ref NttWorld.GetEntityByNetId(itemId);
                ref readonly var itemComp = ref itemNtt.Get<ItemComponent>();
                itemId = itemComp.Id;
            }

            if (!Collections.Shops.TryGetValue(txc.ShopId, out var shopEntry))
            {
                FConsole.WriteLine($"[{nameof(ShopSystem)}]: {ntt.NetId} tried to {(txc.Buy ? "buy" : "sell")} from shop {txc.ShopId} but it doesn't exist in Shops.dat");
                ntt.Remove<RequestShopItemTransactionComponent>();
                return;
            }

            if (!shopEntry.Items.Contains(itemId) && txc.Buy)
            {
                FConsole.WriteLine($"[{nameof(ShopSystem)}]: {ntt.NetId} tried to {(txc.Buy ? "buy" : "sell")} {itemId} but it doesn't exist in the shop {txc.ShopId}");
                ntt.Remove<RequestShopItemTransactionComponent>();
                return;
            }

            if (!Collections.ItemType.TryGetValue(itemId, out var itemEntry))
            {
                FConsole.WriteLine($"[{nameof(ShopSystem)}]: {ntt.NetId} tried to {(txc.Buy ? "buy" : "sell")} {itemId} but it doesn't exist in ItemType.dat");
                ntt.Remove<RequestShopItemTransactionComponent>();
                return;
            }

            if (inv.Money < itemEntry.Price && txc.Buy)
            {
                FConsole.WriteLine($"[{nameof(ShopSystem)}]: {ntt.NetId} tried to buy {itemId} with {inv.Money:C} but it costs {itemEntry.Price:C}");
                ntt.Remove<RequestShopItemTransactionComponent>();
                return;
            }


            for (int i = 0; i < inv.Items.Length; i++)
            {
                ref readonly var itemComp = ref inv.Items[i].Get<ItemComponent>();
                if (itemComp.Id == 0 && txc.Buy || itemComp.Id == itemId && !txc.Buy)
                {
                    if (txc.Buy)
                    {
                        inv.Money -= itemEntry.Price;
                        ref var itemNtt = ref NttWorld.CreateEntity(EntityType.Item);
                        var newItemComp = new ItemComponent(itemNtt.Id, txc.ItemId, itemEntry.Amount, itemEntry.AmountLimit, 0, 0, 0, 0, 0, 0, 0, 0);
                        itemNtt.Set(ref newItemComp);
                        inv.Items[i] = itemNtt;

                        var msg = MsgItemInformation.Create(in itemNtt);
                        ntt.NetSync(ref msg);

                        if (Trace) 
                            Logger.Debug("{0} bought {1} for {2:C} and now has {3:C}", ntt.NetId, txc.ItemId, itemEntry.Price, inv.Money);
                        PrometheusPush.ServerIncome.Inc(itemEntry.Price);
                        PrometheusPush.ShopIncome.Inc(itemEntry.Price);
                        PrometheusPush.ShopPurchases.Inc();
                    }
                    else
                    {
                        Collections.ItemType.TryGetValue(itemComp.Id, out var Info);

                        var money = Info.Price / 3;
                        money = (uint)((double)money * ((float)itemComp.CurrentDurability / itemComp.MaximumDurability));
                        inv.Money += money;

                        ref var itemNtt = ref NttWorld.GetEntityByNetId(txc.ItemId);
                        var def = new DestroyEndOfFrameComponent(itemNtt.Id);
                        itemNtt.Set(ref def);

                        inv.Items[i] = default;

                        var remInv = MsgItem.Create(itemNtt.NetId, itemNtt.NetId, itemNtt.NetId, MsgItemType.RemoveInventory);
                        ntt.NetSync(ref remInv);
                        if (Trace) 
                            Logger.Debug("{0} sold {1} for {2} and now has {3:C}", ntt.NetId, txc.ItemId, money, inv.Money);

                        PrometheusPush.ServerExpenses.Inc(money);
                        PrometheusPush.ShopExpenses.Inc(money);
                        PrometheusPush.ShopSales.Inc();
                    }
                    break;
                }
            }

            ntt.Remove<RequestShopItemTransactionComponent>();
        }
    }
}