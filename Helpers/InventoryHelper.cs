using MagnumOpus.Components;
using MagnumOpus.ECS;

namespace MagnumOpus.Helpers
{
    public static class InventoryHelper
    {
        /// <summary>
        /// Returns the first item in the inventory that matches the given netId.
        /// </summary>
        public static NTT GetInventoryItemByNetIdFrom(in NTT ntt, int netId)
        {
            ref readonly var inv = ref ntt.Get<InventoryComponent>();
            for (int i = 0; i < inv.Items.Length; i++)
                if (inv.Items[i].NetId == netId)
                    return inv.Items[i];
            return default;
        }
        public static bool RemoveNetIdFromInventory(in NTT ntt, int netId, bool destroy = false)
        {
            var item = GetInventoryItemByNetIdFrom(in ntt, netId);
            var removed = RemoveNttFromInventory(in ntt, in item, destroy);
            return removed;
        }        
        public static bool RemoveNttFromInventory(in NTT ntt, in NTT item, bool destroy = false)
        {
            ref readonly var inv = ref ntt.Get<InventoryComponent>();
            var invIdx = Array.IndexOf(inv.Items, item);
            var found = invIdx != -1;

            if(found && destroy)
                item.Set(new DestroyEndOfFrameComponent(item.Id));

            if(found)
                inv.Items[invIdx] = default;

            return found;
        }
    }
}