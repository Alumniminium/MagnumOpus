using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Squiggly;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class DropMoneySystem : NttSystem<PositionComponent, InventoryComponent, RequestDropMoneyComponent>
    {
        public DropMoneySystem() : base("DropMoney", threads: 2) { }

        public override void Update(in NTT ntt, ref PositionComponent pos, ref InventoryComponent inv, ref RequestDropMoneyComponent drc)
        {
            if (inv.Money < drc.Amount)
            {
                ntt.Remove<RequestDropMoneyComponent>();
                Logger.Debug("{ntt} tried to drop {amount} money, but only has {money}", ntt, drc.Amount, inv.Money);
                return;
            }

            inv.Money -= (uint)drc.Amount;

            PrometheusPush.MoneyCount.Inc();
            PrometheusPush.MoneyTotal.Inc(drc.Amount);
            PrometheusPush.ServerExpenses.Inc(drc.Amount);

            ref var itemNtt = ref EntityFactory.MakeMoneyDrop(drc.Amount, ref pos, out var success);
            if (success)
            {
                Collections.SpatialHashs[pos.Map].Add(in itemNtt);
                var dropMsg = MsgFloorItem.Create(in itemNtt, Enums.MsgFloorItemType.Create);
                ntt.NetSync(ref dropMsg, true);
                Logger.Debug("{ntt} dropped {amount} money at {pos}", ntt, drc.Amount, pos.Position);
            }
            else
                Logger.Debug("Failed to create money drop for {ntt}. Amount: {Amount}, Position: {pos}, Map: {Map}", ntt, drc.Amount, pos.Position, pos.Map);

            ntt.Remove<RequestDropMoneyComponent>();
        }
    }
}