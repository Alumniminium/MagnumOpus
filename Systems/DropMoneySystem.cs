using System.Numerics;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Systems
{
    public sealed class DropMoneySystem : NttSystem<PositionComponent, InventoryComponent, RequestDropMoneyComponent>
    {
        public DropMoneySystem() : base("DropMoney", threads: 2, log: true) { }

        public override void Update(in NTT ntt, ref PositionComponent pos, ref InventoryComponent inv, ref RequestDropMoneyComponent drc)
        {
            if (inv.Money < drc.Amount)
            {
                ntt.Remove<RequestDropMoneyComponent>();
                if (IsLogging)
                    FConsole.WriteLine("{ntt} tried to drop {amount} money, but only has {money}", ntt, drc.Amount, inv.Money);
                return;
            }

            inv.Money -= (uint)drc.Amount;

            PrometheusPush.MoneyCount.Inc();
            PrometheusPush.MoneyTotal.Inc(drc.Amount);
            PrometheusPush.ServerExpenses.Inc(drc.Amount);

            var itemNtt = EntityFactory.MakeMoneyDrop(drc.Amount, ref pos);
            if (itemNtt != default)
            {
                var shr = new SpatialHashUpdateComponent(pos.Position, Vector2.Zero, pos.Map, pos.Map, SpacialHashUpdatType.Add);
                itemNtt.Set(ref shr);

                var dropMsg = MsgFloorItem.Create(itemNtt, Enums.MsgFloorItemType.Create);
                ntt.NetSync(ref dropMsg, true);
                if (IsLogging)
                    FConsole.WriteLine("{ntt} dropped {amount} money at {pos}", ntt, drc.Amount, pos.Position);
            }
            else if (IsLogging)
                FConsole.WriteLine("Failed to create money drop for {ntt}. Amount: {Amount}, Position: {pos}, Map: {Map}", ntt, drc.Amount, pos.Position, pos.Map);

            ntt.Remove<RequestDropMoneyComponent>();
        }
    }
}