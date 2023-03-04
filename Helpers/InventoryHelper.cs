using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Helpers
{
    public static class InventoryHelper
    {
        /// <summary>
        /// Returns the first item in the inventory that matches the given netId.
        /// </summary>
        public static NTT GetInventoryItemByNetIdFrom(ref InventoryComponent inv, int netId)
        {
            for (int i = 0; i < inv.Items.Length; i++)
                if (inv.Items[i].NetId == netId)
                    return inv.Items[i];
            return default;
        }
        public static bool RemoveNetIdFromInventory(in NTT ntt, ref InventoryComponent inv, int netId, bool destroy = false, bool netSync = false)
        {
            var item = GetInventoryItemByNetIdFrom(ref inv, netId);
            var removed = RemoveNttFromInventory(in ntt, ref inv, in item, destroy, netSync);
            return removed;
        }        
        public static bool RemoveNttFromInventory(in NTT ntt, ref InventoryComponent inv, in NTT item, bool destroy = false, bool netSync = false)
        {
            var invIdx = Array.IndexOf(inv.Items, item);
            var found = invIdx != -1;

            if(found && destroy)
                item.Set(new DestroyEndOfFrameComponent(item.Id));

            if(found)
                inv.Items[invIdx] = default;

            if (!netSync)
                return found;
                
            var remInv = MsgItem.Create(item.NetId, item.NetId, item.NetId, MsgItemType.RemoveInventory);
            ntt.NetSync(ref remInv);

            return found;
        }
        public static bool HasFreeSpace(ref InventoryComponent inv, int count = 1) => CountItemId(ref inv, 0) >= count;
        public static bool HasItemId(ref InventoryComponent inv, int id) => CountItemId(ref inv, id) > 0;
        public static int CountItemId(ref InventoryComponent inv, int id)
        {
            int count = 0;

            for (int i = 0; i < inv.Items.Length; i++)
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
            for (int i = 0; i < inv.Items.Length; i++)
            {
                ref var item = ref inv.Items[i];
                ref readonly var comp = ref item.Get<ItemComponent>();
                if (comp.Id != id)
                    continue;

                if (destroy)
                    item.Set(new DestroyEndOfFrameComponent(item.Id));

                item = default;
                return true;
            }

            return false;
        }

        public static void SortById(in NTT ntt,ref InventoryComponent inv, bool netSync =false)
        {
            inv.Items = inv.Items.OrderByDescending(x => x.Get<ItemComponent>().Id).ToArray();
           
            if (!netSync)
                return;

            for(int i = 0; i < inv.Items.Length; i++)
            {
                var packet = MsgItem.Create(inv.Items[i].NetId, inv.Items[i].NetId, inv.Items[i].NetId, MsgItemType.RemoveInventory);
                ntt.NetSync(ref packet);
            }
            for(int i = 0; i < inv.Items.Length; i++)
            {
                var packet = MsgItemInformation.Create(in inv.Items[i]);
                ntt.NetSync(ref packet);
            }
        }

        public static bool AddItem(in NTT ntt,ref InventoryComponent inv, in NTT item, bool netSync = false)
        {
            for (int i = 0; i < inv.Items.Length; i++)
            {
                if (inv.Items[i] != default)
                    continue;

                inv.Items[i] = item;
                if (netSync)
                {
                    var packet = MsgItemInformation.Create(in inv.Items[i]);
                    ntt.NetSync(ref packet);
                }
                return true;
            }

            return false;
        }
    }
}