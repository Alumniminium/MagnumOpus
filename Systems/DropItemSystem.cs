using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Squiggly;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class DropItemSystem : NttSystem<PositionComponent, RequestDropItemComponent, InventoryComponent>
    {
        public DropItemSystem() : base("Drop Item", threads: 2) { Trace = false; }
        public override void Update(in NTT ntt, ref PositionComponent pos, ref RequestDropItemComponent rdi, ref InventoryComponent inv)
        {
            if (!InventoryHelper.RemoveNetIdFromInventory(in ntt, ref inv, rdi.ItemNtt.NetId, netSync: true))
            {
                if (Trace) 
                    Logger.Debug("{ntt} tried to drop an Item he does not have in his Inventory at {pos}", ntt, pos.Position);
                ntt.Remove<RequestDropItemComponent>();
                return;
            }

            rdi.ItemNtt.Set(new PositionComponent(rdi.ItemNtt.Id, pos.Position, pos.Map));
            rdi.ItemNtt.Set(new LifeTimeComponent(rdi.ItemNtt.Id, TimeSpan.FromSeconds(30)));
            rdi.ItemNtt.Set(new ViewportComponent(rdi.ItemNtt.Id, 18f));

            Collections.SpatialHashs[pos.Map].Add(in rdi.ItemNtt);

            var msgDropFloor = MsgFloorItem.Create(in rdi.ItemNtt, MsgFloorItemType.Create);
            ntt.NetSync(ref msgDropFloor, true);
                
            ref readonly var item = ref rdi.ItemNtt.Get<ItemComponent>();
            if (Trace)
                Logger.Debug("{ntt} dropped {item} at {pos}", ntt, item.Id, pos.Position);
            
            ntt.Remove<RequestDropItemComponent>();
        }
    }
}