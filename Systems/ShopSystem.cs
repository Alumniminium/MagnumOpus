using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Squiggly;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class ShopSystem : PixelSystem<InventoryComponent, RequestShopItemTransactionComponent>
    {
        public ShopSystem() : base("Shop System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref InventoryComponent inv, ref RequestShopItemTransactionComponent tranaction)
        {
            var itemId = tranaction.ItemId;

            if(!tranaction.Buy)
            {
                ref readonly var itemNtt = ref PixelWorld.GetEntityByNetId(itemId);
                ref readonly var itemComp = ref itemNtt.Get<ItemComponent>();
                itemId = itemComp.Id;
            }

            if (!Collections.Shops.TryGetValue(tranaction.ShopId, out var entry))
            {
                ntt.Remove<RequestShopItemTransactionComponent>();
                return;
            }

            if (!entry.Items.Contains(itemId) && tranaction.Buy)
            {
                ntt.Remove<RequestShopItemTransactionComponent>();
                return;
            }

            if (!Collections.ItemType.TryGetValue(itemId, out var itemEntry))
            {
                ntt.Remove<RequestShopItemTransactionComponent>();
                return;
            }

            if (inv.Money < itemEntry.Price && tranaction.Buy)
            {
                ntt.Remove<RequestShopItemTransactionComponent>();
                return;
            }


            for (int i = 0; i < inv.Items.Length; i++)
            {
                ref readonly var itemComp = ref inv.Items[i].Get<ItemComponent>();
                if (itemComp.Id == 0 && tranaction.Buy || itemComp.Id == itemId && !tranaction.Buy)
                {
                    if(tranaction.Buy)
                    {
                        inv.Money -= itemEntry.Price;
                        ref var itemNtt = ref PixelWorld.CreateEntity(EntityType.Item);
                        var newItemComp = new ItemComponent(itemNtt.Id, tranaction.ItemId, itemEntry.Amount, itemEntry.AmountLimit, 0, 0, 0, 0, 0, 0, 0, 0);
                        itemNtt.Add(ref newItemComp);
                        inv.Items[i] = itemNtt;

                        var msg = MsgItemInformation.Create(in itemNtt);
                        ntt.NetSync(ref msg);
                    }
                    else
                    {
                        inv.Money += itemEntry.Price;
                        ref var itemNtt = ref PixelWorld.GetEntityByNetId(tranaction.ItemId);
                        var def = new DestroyEndOfFrameComponent();
                        itemNtt.Add(ref def);

                        var msg = MsgItem.Create(itemNtt.NetId, 0, 0, PixelWorld.Tick, MsgItemType.RemoveInventory);
                        ntt.NetSync(ref msg);
                    }
                    var moneyMsg = MsgUserAttrib.Create(ntt.NetId, inv.Money, MsgUserAttribType.MoneyInventory);
                    ntt.NetSync(ref moneyMsg);
                    break;
                }
            }

            ntt.Remove<RequestShopItemTransactionComponent>();
        }
    }
}