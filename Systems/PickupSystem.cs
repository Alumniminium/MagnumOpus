using System.Net.Security;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Networking;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class PickupSystem : PixelSystem<PositionComponent, InventoryComponent, PickupRequestComponent>
    {
        public PickupSystem() : base("Pickup System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref PositionComponent pos, ref InventoryComponent inv, ref PickupRequestComponent pic)
        {
            if (pic.Item.Has<MoneyRewardComponent>())
            {
                ref readonly var rew = ref pic.Item.Get<MoneyRewardComponent>();
                inv.Money += (uint)rew.Amount;
                var ded = new DestroyEndOfFrameComponent(pic.Item.Id);
                pic.Item.Add(ref ded);

                var moneyMsg = MsgUserAttrib.Create(ntt.NetId, inv.Money, Enums.MsgUserAttribType.MoneyInventory);
                ntt.NetSync(ref moneyMsg);

                var moneyTxtMsg = MsgText.Create(in ntt, $"You picked up {rew.Amount} gold", Enums.MsgTextType.Action);
                var moneyTxtSerialized = Co2Packet.Serialize(ref moneyTxtMsg, moneyTxtMsg.Size);
                ntt.NetSync(in moneyTxtSerialized);

                if(rew.Amount > 1000)
                {
                    var moneyActionMsg = MsgAction.Create(ntt.NetId, 0, 0, 0, 0, Enums.MsgActionType.GetMoney);
                    ntt.NetSync(ref moneyActionMsg, true);
                }
            }
            else
            {

                var emptyIdx = Array.IndexOf(inv.Items, default);

                if (emptyIdx == -1)
                {
                    ntt.Remove<PickupRequestComponent>();
                    FConsole.WriteLine($"[{nameof(PickupSystem)}]: {ntt.NetId} tried to pick up {pic.Item.NetId} but their inventory is full");
                    return;
                }

                pic.Item.Remove<PositionComponent>();
                inv.Items[emptyIdx] = pic.Item;


                inv.Items = inv.Items.OrderByDescending(x => x.Get<ItemComponent>().Id).ToArray();

                for (var i = 0; i < inv.Items.Length; i++)
                {
                    if (inv.Items[i].Id != 0)
                    {
                        var rmMsg = MsgItem.Create(inv.Items[i].NetId, inv.Items[i].NetId, inv.Items[i].NetId, 0, Enums.MsgItemType.RemoveInventory);
                        ntt.NetSync(ref rmMsg);
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
            }

            Game.Grids[pos.Map].Remove(in pic.Item);

            var delFloorMsg = MsgFloorItem.Create(in pic.Item, Enums.MsgFloorItemType.Delete);
            ntt.NetSync(ref delFloorMsg, true);

            FConsole.WriteLine($"[{nameof(PickupSystem)}]: {ntt.NetId} picked up {pic.Item.NetId}");
            ntt.Remove<PickupRequestComponent>();
        }
    }
}