using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class DropSystem : PixelSystem<PositionComponent, RequestDropItemComponent>
    {
        public DropSystem() : base("Drop System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref PositionComponent pos, ref RequestDropItemComponent drc)
        {
            ref var itemNtt = ref PixelWorld.GetEntityByNetId(drc.ItemNetId);
            ref var item = ref itemNtt.Get<ItemComponent>();

            if(ntt.Has<InventoryComponent>())
            {
                ref var inv = ref ntt.Get<InventoryComponent>();
                var invIdx = Array.IndexOf(inv.Items, itemNtt);
                inv.Items[invIdx] = default;
                var msg = MsgItem.Create(itemNtt.NetId, itemNtt.NetId, itemNtt.NetId, MsgItemType.RemoveInventory);
                ntt.NetSync(ref msg);
            }

            Game.Grids[pos.Map].Add(in itemNtt, ref pos);

            var dropPos = new PositionComponent(itemNtt.Id, pos.Position, pos.Map);
            itemNtt.Set(ref dropPos);

            var dropMsg = MsgFloorItem.Create(in itemNtt, Enums.MsgFloorItemType.Create);
            ntt.NetSync(ref dropMsg, true);
            FConsole.WriteLine($"[{nameof(DropSystem)}] {ntt.NetId} dropped {item.Id} at {pos.Position} on map {pos.Map}.");
            ntt.Remove<RequestDropItemComponent>();
        }
    }
}