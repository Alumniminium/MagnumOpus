using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class DropItemSystem : PixelSystem<PositionComponent, RequestDropItemComponent>
    {
        public DropItemSystem() : base("Drop Item System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref PositionComponent pos, ref RequestDropItemComponent rdi)
        {
            ref var item = ref rdi.ItemNtt.Get<ItemComponent>();

            if (ntt.Has<InventoryComponent>())
            {
                ref var inv = ref ntt.Get<InventoryComponent>();
                var invIdx = Array.IndexOf(inv.Items, rdi.ItemNtt);
                inv.Items[invIdx] = default;
                var msg = MsgItem.Create(rdi.ItemNtt.NetId, rdi.ItemNtt.NetId, rdi.ItemNtt.NetId, MsgItemType.RemoveInventory);
                ntt.NetSync(ref msg);
            }

            Game.Grids[pos.Map].Add(in rdi.ItemNtt, ref pos);

            var dropPos = new PositionComponent(rdi.ItemNtt.Id, pos.Position, pos.Map);
            rdi.ItemNtt.Set(ref dropPos);

            var dropMsg = MsgFloorItem.Create(in rdi.ItemNtt, MsgFloorItemType.Create);
            ntt.NetSync(ref dropMsg, true);
            FConsole.WriteLine($"[{nameof(DropItemSystem)}] {ntt.NetId} dropped {item.Id} at {pos.Position} on map {pos.Map}.");
            ntt.Remove<RequestDropItemComponent>();
        }
    }
}