using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class DropItemSystem : PixelSystem<PositionComponent, RequestDropItemComponent, InventoryComponent>
    {
        public DropItemSystem() : base("Drop Item System", threads: 1) { }
        public override void Update(in PixelEntity ntt, ref PositionComponent pos, ref RequestDropItemComponent rdi, ref InventoryComponent inv)
        {
            var removed = InventoryHelper.RemoveNetIdFromInventory(in ntt, rdi.ItemNtt.NetId);

            if (!removed)
            {
                FConsole.WriteLine($"[{nameof(DropItemSystem)}] {ntt.NetId} tried to drop an Item he does not have in his Inventory at {pos.Position} on map {pos.Map}.");
                ntt.Remove<RequestDropItemComponent>();
                return;
            }

            ref var item = ref rdi.ItemNtt.Get<ItemComponent>();
            
            var dropPos = new PositionComponent(rdi.ItemNtt.Id, pos.Position, pos.Map);
            rdi.ItemNtt.Set(ref dropPos);

            Game.Grids[pos.Map].Add(in rdi.ItemNtt, ref pos);

            var msgRemoveInv = MsgItem.Create(rdi.ItemNtt.NetId, rdi.ItemNtt.NetId, rdi.ItemNtt.NetId, MsgItemType.RemoveInventory);
            var msgDropFloor = MsgFloorItem.Create(in rdi.ItemNtt, MsgFloorItemType.Create);
            ntt.NetSync(ref msgRemoveInv);
            ntt.NetSync(ref msgDropFloor, true);

            FConsole.WriteLine($"[{nameof(DropItemSystem)}] {ntt.NetId} dropped {item.Id} at {pos.Position} on map {pos.Map}.");
            ntt.Remove<RequestDropItemComponent>();
        }
    }
}