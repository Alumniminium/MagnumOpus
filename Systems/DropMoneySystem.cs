using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class DropMoneySystem : NttSystem<PositionComponent, InventoryComponent, RequestDropMoneyComponent>
    {
        public DropMoneySystem() : base("DropMoney", threads: Environment.ProcessorCount) { }

        public override void Update(in NTT ntt, ref PositionComponent pos, ref InventoryComponent inv, ref RequestDropMoneyComponent drc)
        {
            if (inv.Money < drc.Amount)
            {
                ntt.Remove<RequestDropMoneyComponent>();
                return;
            }

            inv.Money -= (uint)drc.Amount;

            ref var itemNtt = ref EntityFactory.MakeMoneyDrop(drc.Amount, ref pos, out var success);
            if (success)
            {
                Game.Grids[pos.Map].Add(in itemNtt, ref pos);
                var dropMsg = MsgFloorItem.Create(in itemNtt, Enums.MsgFloorItemType.Create);
                ntt.NetSync(ref dropMsg, true);
            }
            else
                FConsole.WriteLine($"[{nameof(DropMoneySystem)}] Failed to create money drop for {ntt.NetId}. Amount: {drc.Amount}, Position: {pos.Position}, Map: {pos.Map}");

            ntt.Remove<RequestDropMoneyComponent>();
        }
    }
}