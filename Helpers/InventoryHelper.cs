using MagnumOpus.Components;
using MagnumOpus.ECS;

namespace MagnumOpus.Helpers
{
    public static class InventoryHelper
    {
        /// <summary>
        /// Returns the first item in the inventory that matches the given netId.
        /// </summary>
        public static PixelEntity GetInventoryItemByNetIdFrom(in PixelEntity ntt, int netId)
        {
            ref readonly var inv = ref ntt.Get<InventoryComponent>();
            for (int i = 0; i < inv.Items.Length; i++)
                if (inv.Items[i].NetId == netId)
                    return inv.Items[i];
            return default;
        }
        public static bool RemoveNetIdFromInventory(in PixelEntity ntt, int netId)
        {
            ref readonly var inv = ref ntt.Get<InventoryComponent>();
           
            for (int i = 0; i < inv.Items.Length; i++)
                if (inv.Items[i].NetId == netId)
                {
                    inv.Items[i] = default;
                    return true;
                }
            return false;
        }        
        public static void RemoveNttFromInventory(in PixelEntity ntt, in PixelEntity item)
        {
            ref readonly var inv = ref ntt.Get<InventoryComponent>();
            var invIdx = Array.IndexOf(inv.Items, item);
            if(invIdx != -1)
            inv.Items[invIdx] = default;
        }
    }
}