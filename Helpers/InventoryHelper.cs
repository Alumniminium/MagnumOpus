using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Helpers
{
    public static class InventoryHelper
    {
        public static NTT GetInventoryItemByNetIdFrom(ref InventoryComponent inv, int netId)
        {
            for (var i = 0; i < inv.Items.Length; i++)
                if (inv.Items[i].Id == netId)
                    return inv.Items[i];
            return default;
        }

        public static bool RemoveNetIdFromInventory(NTT ntt, ref InventoryComponent inv, int netId, bool destroy = false, bool netSync = false)
        {
            var itemNtt = GetInventoryItemByNetIdFrom(ref inv, netId);
            var removed = RemoveNttFromInventory(ntt, ref inv, itemNtt, destroy, netSync);
            return removed;
        }

        public static bool RemoveNttFromInventory(NTT ntt, ref InventoryComponent inv, NTT item, bool destroy = false, bool netSync = false)
        {
            var invIdx = Array.IndexOf(inv.Items, item);
            var found = invIdx != -1;

            if (found && destroy)
                item.Set<DestroyEndOfFrameComponent>();

            if (found)
                inv.Items[invIdx] = default;

            if (!netSync)
                return found;

            var remInv = MsgItem.Create(item.Id, item.Id, item.Id, MsgItemType.RemoveInventory);
            ntt.NetSync(ref remInv);

            return found;
        }

        public static bool HasFreeSpace(ref InventoryComponent inv, int count = 1) => CountItemId(ref inv, 0) >= count;
        public static bool HasItemId(ref InventoryComponent inv, int id) => CountItemId(ref inv, id) > 0;
        public static int CountItemId(ref InventoryComponent inv, int id)
        {
            var count = 0;

            for (var i = 0; i < inv.Items.Length; i++)
            {
                ref readonly var comp = ref inv.Items[i].Get<ItemComponent>();
                if (comp.Id != id)
                    continue;

                count++;
            }

            return count;
        }

        public static bool RemoveItemId(ref InventoryComponent inv, int id, bool destroy = false)
        {
            for (var i = 0; i < inv.Items.Length; i++)
            {
                ref var item = ref inv.Items[i];
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

        public static void SortById(NTT ntt, ref InventoryComponent inv, bool netSync = false)
        {
            // Replace the use of LINQ's OrderByDescending with an inline sorting function
            Array.Sort(inv.Items, (x, y) => y.Get<ItemComponent>().Id.CompareTo(x.Get<ItemComponent>().Id));

            if (!netSync)
                return;

            for (var i = 0; i < inv.Items.Length; i++)
            {
                var packet = MsgItem.Create(inv.Items[i].Id, inv.Items[i].Id, inv.Items[i].Id, MsgItemType.RemoveInventory);
                ntt.NetSync(ref packet);
            }
            for (var i = 0; i < inv.Items.Length; i++)
            {
                var packet = MsgItemInformation.Create(inv.Items[i]);
                ntt.NetSync(ref packet);
            }
        }

        public static bool AddItem(NTT ntt, ref InventoryComponent inv, in NTT item, bool netSync = false)
        {
            for (var i = 0; i < inv.Items.Length; i++)
            {
                if (inv.Items[i] != default)
                    continue;

                inv.Items[i] = item;
                if (netSync)
                {
                    var packet = MsgItemInformation.Create(inv.Items[i]);
                    ntt.NetSync(ref packet);
                }
                return true;
            }

            return false;
        }

        internal static int CountItems(ref InventoryComponent inv)
        {
            var itemCount = 0;
            foreach (var item in inv.Items)
                if (item.Id != 0)
                    itemCount++;
            return itemCount;
        }
    }
}