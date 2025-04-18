using System.Numerics;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Systems
{
    public sealed class DropItemSystem : NttSystem<PositionComponent, RequestDropItemComponent, InventoryComponent>
    {
        public DropItemSystem() : base("Drop Item", threads: 2) { IsLogging = false; }
        public override void Update(in NTT ntt, ref PositionComponent pos, ref RequestDropItemComponent rdi, ref InventoryComponent inv)
        {
            if (!InventoryHelper.RemoveNetIdFromInventory(ntt, ref inv, rdi.ItemNtt.Id, netSync: true))
            {
                if (IsLogging)
                    FConsole.WriteLine("{ntt} tried to drop an Item he does not have in his Inventory at {pos}", ntt, pos.Position);
                ntt.Remove<RequestDropItemComponent>();
                return;
            }

            rdi.ItemNtt.Set(new PositionComponent(pos.Position, pos.Map));
            rdi.ItemNtt.Set(new LifeTimeComponent(TimeSpan.FromSeconds(30)));
            rdi.ItemNtt.Set(new ViewportComponent(18f));

            var shr = new SpatialHashUpdateComponent(pos.Position, Vector2.Zero, pos.Map, pos.Map, SpacialHashUpdatType.Add);
            rdi.ItemNtt.Set(ref shr);


            var msgDropFloor = MsgFloorItem.Create(in rdi.ItemNtt, MsgFloorItemType.Create);
            ntt.NetSync(ref msgDropFloor, true);

            ref readonly var item = ref rdi.ItemNtt.Get<ItemComponent>();
            if (IsLogging)
                FConsole.WriteLine("{ntt} dropped {item} at {pos}", ntt, item.Id, pos.Position);

            ntt.Remove<RequestDropItemComponent>();
        }
    }
}