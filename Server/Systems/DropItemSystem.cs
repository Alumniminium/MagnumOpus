using System.Numerics;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Systems
{
    /// <summary>
    /// Handles item dropping mechanics including inventory validation, ground placement, and spatial updates.
    /// Manages the transition of items from player inventories to ground entities with limited lifetime.
    /// </summary>
    public sealed class DropItemSystem : NttSystem<PositionComponent, RequestDropItemComponent, InventoryComponent>
    {
        /// <summary>
        /// Initializes the DropItemSystem with limited threading for item drop processing.
        /// </summary>
        public DropItemSystem() : base("Drop Item", threads: 2) { IsLogging = false; }
        
        /// <summary>
        /// Processes item drop requests, removing items from inventory and placing them on the ground.
        /// </summary>
        /// <param name="ntt">The entity dropping the item</param>
        /// <param name="pos">Position component for determining drop location</param>
        /// <param name="rdi">Request drop item component containing the item to drop</param>
        /// <param name="inv">Inventory component to remove the item from</param>
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

            var spatialUpdate = new SpatialHashUpdateComponent(pos.Position, Vector2.Zero, pos.Map, pos.Map, SpacialHashUpdatType.Add);
            rdi.ItemNtt.Set(ref spatialUpdate);

            var floorItemPacket = MsgFloorItem.Create(in rdi.ItemNtt, MsgFloorItemType.Create);
            ntt.NetSync(ref floorItemPacket, true);

            ref readonly var droppedItem = ref rdi.ItemNtt.Get<ItemComponent>();
            if (IsLogging)
                FConsole.WriteLine("{ntt} dropped {item} at {pos}", ntt, droppedItem.Id, pos.Position);

            ntt.Remove<RequestDropItemComponent>();
        }
    }
}