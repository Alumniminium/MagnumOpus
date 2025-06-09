using System.Numerics;
using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;
using NttECS.ECS;

namespace MagnumOpus.Systems;

public sealed class DropItemSystem : NttSystem<PositionComponent, RequestDropItemComponent, InventoryComponent>
{
    public DropItemSystem() : base("Drop Item", threads: 1, log: false) { IsLogging = false; }

    // Handles item dropping from player inventories to the ground. When a player requests to drop
    // an item, the system validates the item exists in their inventory, removes it, and creates a
    // ground entity at the player's position. The dropped item has a 30-second lifetime before
    // automatic cleanup and broadcasts its creation to nearby players for visibility.
    public override void Update(in NTT ntt, ref PositionComponent pos, ref RequestDropItemComponent rdi, ref InventoryComponent inv)
    {
        // === VALIDATE ITEM IN INVENTORY ===
        // Ensure the player actually has this item before dropping it
        if (!InventoryHelper.RemoveNetIdFromInventory(ntt, ref inv, rdi.ItemNtt.Id, netSync: true))
        {
            if (IsLogging)
                FConsole.WriteLine("{ntt} tried to drop an item they don't have at {pos}", ntt, pos.Position);

            ntt.Remove<RequestDropItemComponent>();
            return;
        }

        // === CREATE GROUND ITEM ENTITY ===
        // Set up the dropped item as a ground entity with position and lifetime
        rdi.ItemNtt.Set(new PositionComponent(pos.Position, pos.Map));
        rdi.ItemNtt.Set(new LifeTimeComponent(TimeSpan.FromSeconds(30))); // 30 second lifetime
        rdi.ItemNtt.Set(new ViewportComponent(18f)); // Visibility range

        // Add to spatial hash for efficient queries
        var spatialUpdate = new SpatialHashUpdateComponent(pos.Position, Vector2.Zero, pos.Map, pos.Map, SpacialHashUpdatType.Add);
        rdi.ItemNtt.Set(ref spatialUpdate);

        // === BROADCAST TO NEARBY PLAYERS ===
        // Notify players in the area that a new item appeared
        var floorItemPacket = MsgFloorItem.Create(in rdi.ItemNtt, MsgFloorItemType.Create);
        ntt.NetSync(ref floorItemPacket, true);

        if (IsLogging)
        {
            ref readonly var droppedItem = ref rdi.ItemNtt.Get<ItemComponent>();
            FConsole.WriteLine("{ntt} dropped item {item} at {pos}", ntt, droppedItem.Id, pos.Position);
        }

        // Clean up the drop request component
        ntt.Remove<RequestDropItemComponent>();
    }
}