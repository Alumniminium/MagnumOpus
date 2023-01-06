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
            for (var i = 0; i < inv.Items.Length; i++)
            {
                if (inv.Items[i].Id != 0)
                {
                    var dropMsg = MsgItem.Create(inv.Items[i].NetId, inv.Items[i].NetId, inv.Items[i].NetId, 0, Enums.MsgItemType.RemoveInventory);
                    ntt.NetSync(ref dropMsg);
                }
            }

            for(int i = 0; i < inv.Items.Length; i++)
            {
                if(inv.Items[i].Id == 0)
                {
                    Game.Grids[pos.Map].Remove(in pic.Item);
                    pic.Item.Remove<PositionComponent>();
                    inv.Items[i] = pic.Item;

                    var dropMsg = MsgFloorItem.Create(in pic.Item, Enums.MsgFloorItemType.Delete);
                    ntt.NetSync(ref dropMsg, true);
                    break;
                }
            }

            inv.Items = inv.Items.OrderByDescending(x => x.Get<ItemComponent>().Id).ToArray();

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