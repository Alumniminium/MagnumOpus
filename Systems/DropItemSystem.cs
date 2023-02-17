using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class DropItemSystem : NttSystem<PositionComponent, RequestDropItemComponent, InventoryComponent>
    {
        public DropItemSystem() : base("Drop Item", threads:2) { }
        public override void Update(in NTT ntt, ref PositionComponent pos, ref RequestDropItemComponent rdi, ref InventoryComponent inv)
        {
            if (!InventoryHelper.RemoveNetIdFromInventory(in ntt, rdi.ItemNtt.NetId))
            {
                if (Trace)
                FConsole.WriteLine($"[{nameof(DropItemSystem)}] {ntt.NetId} tried to drop an Item he does not have in his Inventory at {pos.Position} on map {pos.Map}.");
                ntt.Remove<RequestDropItemComponent>();
                return;
            }

            ref var item = ref rdi.ItemNtt.Get<ItemComponent>();
            var dropPos = new PositionComponent(rdi.ItemNtt.Id, pos.Position, pos.Map);
            var ltc = new LifeTimeComponent(rdi.ItemNtt.Id, TimeSpan.FromSeconds(30));
            var vwp = new ViewportComponent(rdi.ItemNtt.Id, 18f);
            rdi.ItemNtt.Set(ref dropPos);
            rdi.ItemNtt.Set(ref ltc);
            rdi.ItemNtt.Set(ref vwp);

            Game.SpatialHashs[pos.Map].Add(in rdi.ItemNtt);

            var msgRemoveInv = MsgItem.Create(rdi.ItemNtt.NetId, rdi.ItemNtt.NetId, rdi.ItemNtt.NetId, MsgItemType.RemoveInventory);
            var msgDropFloor = MsgFloorItem.Create(in rdi.ItemNtt, MsgFloorItemType.Create);
            ntt.NetSync(ref msgRemoveInv);
            ntt.NetSync(ref msgDropFloor, true);

                if (Trace)
            FConsole.WriteLine($"[{nameof(DropItemSystem)}] {ntt.NetId} dropped {item.Id} at {pos.Position} on map {pos.Map}.");
            ntt.Remove<RequestDropItemComponent>();
        }
    }
}