using System.Net.Security;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Helpers;
using MagnumOpus.Networking;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Squiggly;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class PickupSystem : NttSystem<PositionComponent, InventoryComponent, PickupRequestComponent>
    {
        public PickupSystem() : base("Pickup", threads: 2) { }

        public override void Update(in NTT ntt, ref PositionComponent pos, ref InventoryComponent inv, ref PickupRequestComponent pic)
        {
            if (pic.Item.Has<MoneyRewardComponent>())
            {
                ref readonly var rew = ref pic.Item.Get<MoneyRewardComponent>();
                inv.Money += (uint)rew.Amount;

                var moneyTxtMsg = MsgText.Create(in ntt, $"You picked up {rew.Amount} gold", Enums.MsgTextType.TopLeft);
                ntt.NetSync(ref moneyTxtMsg);

                if(rew.Amount > 1000)
                {
                    var moneyActionMsg = MsgAction.Create(ntt.NetId, ntt.NetId, 0, 0, 0, Enums.MsgActionType.GetMoney);
                    ntt.NetSync(ref moneyActionMsg, true);
                }

                var ded = new DestroyEndOfFrameComponent(pic.Item.Id);
                pic.Item.Set(ref ded);
            }
            else
            {
                if(!InventoryHelper.HasFreeSpace(ref inv))
                {
                    ntt.Remove<PickupRequestComponent>();
                    return;
                }

                pic.Item.Remove<PositionComponent>();
                pic.Item.Remove<LifeTimeComponent>();
                pic.Item.Remove<DestroyEndOfFrameComponent>();

                InventoryHelper.AddItem(in ntt, ref inv, in pic.Item);
                InventoryHelper.SortById(in ntt, ref inv, netSync: true);
            }

            Collections.SpatialHashs[pos.Map].Remove(in pic.Item, ref pos);
            
            var delFloorMsg = MsgFloorItem.Create(in pic.Item, Enums.MsgFloorItemType.Delete);
            ntt.NetSync(ref delFloorMsg, true);

            if (Trace) 
                Logger.Debug("{0} picked up {1}", ntt, pic.Item);
            ntt.Remove<PickupRequestComponent>();
        }
    }
}