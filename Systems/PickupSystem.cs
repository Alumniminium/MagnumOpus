using System.Net.Security;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class PickupSystem : PixelSystem<PositionComponent, InventoryComponent, PickupRequestComponent>
    {
        public PickupSystem() : base("Pickup System", threads: 1) { }

        public override void Update(in PixelEntity ntt,ref PositionComponent pos, ref InventoryComponent inv, ref PickupRequestComponent pic)
        {
            var emptyIdx = Array.IndexOf(inv.Items, default);

            if (emptyIdx == -1)
            {
                ntt.Remove<PickupRequestComponent>();
                return;
            }

            Game.Grids[pos.Map].Remove(in pic.Item);
            pic.Item.Remove<PositionComponent>();
            inv.Items[emptyIdx] = pic.Item;

            var dropMsg = MsgFloorItem.Create(in pic.Item, Enums.MsgFloorItemType.Delete);
            ntt.NetSync(ref dropMsg, true);
            
            inv.Items = inv.Items.OrderByDescending(x => x.Get<ItemComponent>().Id).ToArray();
            
            for (var i = 0; i < inv.Items.Length; i++)
            {
                if (inv.Items[i].Id != 0)
                {
                    var dropMsg = MsgItem.Create(inv.Items[i].NetId, inv.Items[i].NetId, inv.Items[i].NetId, 0, Enums.MsgItemType.RemoveInventory);
                    ntt.NetSync(ref dropMsg);
                }
            }
            for (var i = 0; i < inv.Items.Length; i++)
            {
                if (inv.Items[i].Id != 0)
                {
                    var addMsg = MsgItemInformation.Create(in inv.Items[i]);
                    ntt.NetSync(ref addMsg);
                }
            }
           
            ntt.Remove<PickupRequestComponent>();
        }
    }
}