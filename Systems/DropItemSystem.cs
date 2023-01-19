using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class DropItemSystem : PixelSystem<PositionComponent, RequestDropItemComponent, InventoryComponent>
    {
        public DropItemSystem() : base("Drop Item System", threads: 1) { }
        public override void Update(in PixelEntity ntt, ref PositionComponent pos, ref RequestDropItemComponent rdi, ref InventoryComponent inv)
        {
            ref var item = ref rdi.ItemNtt.Get<ItemComponent>();
            var invIdx = Array.IndexOf(inv.Items, rdi.ItemNtt);

            if(invIdx == -1)
            {
                FConsole.WriteLine($"[{nameof(DropItemSystem)}] {ntt.Id} - {rdi.ItemNtt} - Item not found in inventory.");
                ntt.Remove<RequestDropItemComponent>();
                return;
            }


            inv.Items[invIdx] = default;

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

        // TODO:
        // - Add a check to see if the item is in the inventory
    }
}