using MagnumOpus.Components;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;
using NttECS.ECS;

namespace MagnumOpus.Helpers
{
    /// <summary>
    /// Utility functions for inventory management including item operations, space validation, and network synchronization.
    /// Provides comprehensive inventory manipulation with automatic client updates and entity lifecycle management.
    /// </summary>
    public static class InventoryHelper
    {
        /// <summary>
        /// Retrieves an inventory item entity by its network ID.
        /// </summary>
        /// <param name="inv">Inventory component to search</param>
        /// <param name="netId">Network ID of the item to find</param>
        /// <returns>Item entity or default if not found</returns>
        public static NTT GetInventoryItemByNetIdFrom(ref InventoryComponent inv, int netId)
        {
            for (var i = 0; i < inv.Items.Length; i++)
                if (inv.Items.Span[i].Id == netId)
                    return inv.Items.Span[i];
            return default;
        }

        /// <summary>
        /// Removes an item from inventory by network ID with optional destruction and client synchronization.
        /// </summary>
        /// <param name="owner">Entity that owns the inventory</param>
        /// <param name="inv">Inventory component to modify</param>
        /// <param name="netId">Network ID of item to remove</param>
        /// <param name="destroy">Whether to mark item for destruction</param>
        /// <param name="netSync">Whether to send removal packet to client</param>
        /// <returns>True if item was found and removed</returns>
        public static bool RemoveNetIdFromInventory(NTT owner, ref InventoryComponent inv, int netId, bool destroy = false, bool netSync = false)
        {
            var itemNtt = GetInventoryItemByNetIdFrom(ref inv, netId);
            var removed = RemoveNttFromInventory(owner, ref inv, itemNtt, destroy, destroy || netSync);
            return removed;
        }

        /// <summary>
        /// Removes a specific item entity from inventory with optional destruction and client synchronization.
        /// </summary>
        /// <param name="owner">Entity that owns the inventory</param>
        /// <param name="inv">Inventory component to modify</param>
        /// <param name="item">Item entity to remove</param>
        /// <param name="destroy">Whether to mark item for destruction</param>
        /// <param name="netSync">Whether to send removal packet to client</param>
        /// <returns>True if item was found and removed</returns>
        public static bool RemoveNttFromInventory(NTT owner, ref InventoryComponent inv, NTT item, bool destroy = false, bool netSync = false)
        {
            var invIdx = -1;
            for (var i = 0; i < inv.Items.Length; i++)
            {
                if (item.Id == inv.Items.Span[i].Id)
                {
                    invIdx = i;
                    break;
                }
            }

            var found = invIdx != -1;

            if (found && destroy)
                item.Set<DestroyEndOfFrameComponent>();

            if (found)
                inv.Items.Span[invIdx] = default;

            if (!netSync)
                return found;

            var remInv = MsgItem.Create(item.Id, item.Id, item.Id, MsgItemType.RemoveInventory);
            owner.NetSync(ref remInv);

            return found;
        }

        /// <summary>
        /// Checks if inventory has the specified number of free slots available.
        /// </summary>
        /// <param name="inv">Inventory component to check</param>
        /// <param name="count">Number of free slots required</param>
        /// <returns>True if enough free space is available</returns>
        public static bool HasFreeSpace(ref InventoryComponent inv, int count = 1) => CountItemId(ref inv, 0) >= count;
        
        /// <summary>
        /// Checks if inventory contains an item with the specified network ID.
        /// </summary>
        /// <param name="inv">Inventory component to search</param>
        /// <param name="netId">Network ID to search for</param>
        /// <returns>True if item with network ID is found</returns>
        public static bool HasItemNetId(ref InventoryComponent inv, int netId)
        {
            for (var i = 0; i < inv.Items.Length; i++)
                if (inv.Items.Span[i].Id == netId)
                    return true;
            return false;
        }
        /// <summary>
        /// Checks if inventory contains any item with the specified item ID.
        /// </summary>
        /// <param name="inv">Inventory component to search</param>
        /// <param name="id">Item ID to search for</param>
        /// <returns>True if any item with the ID is found</returns>
        public static bool HasItemId(ref InventoryComponent inv, int id) => CountItemId(ref inv, id) > 0;
        
        /// <summary>
        /// Counts the number of items with the specified item ID in inventory.
        /// </summary>
        /// <param name="inv">Inventory component to search</param>
        /// <param name="id">Item ID to count (0 counts empty slots)</param>
        /// <returns>Number of items with the specified ID</returns>
        public static int CountItemId(ref InventoryComponent inv, int id)
        {
            var count = 0;

            for (var i = 0; i < inv.Items.Length; i++)
            {
                ref readonly var comp = ref inv.Items.Span[i].Get<ItemComponent>();
                if (comp.Id != id)
                    continue;

                count++;
            }

            return count;
        }
        /// <summary>
        /// Removes the first item with the specified item ID from inventory.
        /// </summary>
        /// <param name="inv">Inventory component to modify</param>
        /// <param name="id">Item ID to remove</param>
        /// <param name="destroy">Whether to mark item for destruction</param>
        /// <returns>True if item was found and removed</returns>
        public static bool RemoveItemId(ref InventoryComponent inv, int id, bool destroy = false)
        {
            for (var i = 0; i < inv.Items.Length; i++)
            {
                ref var item = ref inv.Items.Span[i];
                ref readonly var comp = ref item.Get<ItemComponent>();
                if (comp.Id != id)
                    continue;

                if (destroy)
                    item.Set<DestroyEndOfFrameComponent>();

                item = default;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Refreshes inventory display on client by removing and re-adding all items.
        /// Used for inventory sorting or complete refresh operations.
        /// </summary>
        /// <param name="owner">Entity that owns the inventory</param>
        /// <param name="inv">Inventory component to refresh</param>
        /// <param name="netSync">Whether to send refresh packets to client</param>
        public static void SortById(NTT owner, ref InventoryComponent inv, bool netSync = false)
        {
            if (!netSync)
                return;

            for (var i = 0; i < inv.Items.Length; i++)
            {
                var packet = MsgItem.Create(inv.Items.Span[i].Id, inv.Items.Span[i].Id, inv.Items.Span[i].Id, MsgItemType.RemoveInventory);
                owner.NetSync(ref packet);
            }
            for (var i = 0; i < inv.Items.Length; i++)
            {
                var packet = MsgItemInformation.Create(inv.Items.Span[i]);
                owner.NetSync(ref packet);
            }
        }

        /// <summary>
        /// Adds an item entity to the first available inventory slot with optional client synchronization.
        /// </summary>
        /// <param name="owner">Entity that owns the inventory</param>
        /// <param name="inv">Inventory component to modify</param>
        /// <param name="item">Item entity to add</param>
        /// <param name="netSync">Whether to send addition packet to client</param>
        /// <returns>True if item was successfully added</returns>
        public static bool AddItem(NTT owner, ref InventoryComponent inv, in NTT item, bool netSync = false)
        {
            for (var i = 0; i < inv.Items.Length; i++)
            {
                if (inv.Items.Span[i] != default)
                    continue;

                inv.Items.Span[i] = item;
                if (netSync)
                {
                    var packet = MsgItemInformation.Create(inv.Items.Span[i]);
                    owner.NetSync(ref packet);
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Counts the total number of non-empty items in inventory.
        /// </summary>
        /// <param name="inv">Inventory component to count</param>
        /// <returns>Number of items currently in inventory</returns>
        internal static int CountItems(ref InventoryComponent inv)
        {
            var itemCount = 0;
            foreach (var item in inv.Items.Span)
                if (item.Id != 0)
                    itemCount++;
            return itemCount;
        }
    }
}