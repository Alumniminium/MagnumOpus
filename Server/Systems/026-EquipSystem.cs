using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;
using NttECS.ECS;

namespace MagnumOpus.Systems;

public sealed class EquipSystem : NttSystem<InventoryComponent, EquipmentComponent, RequestChangeEquipComponent>
{
    public EquipSystem() : base("Equip", threads: 1, log: false) { }

    // Handles equipment changes including equipping and unequipping items between inventory and
    // equipment slots. When equipping, the system moves items from inventory to equipment slots
    // and handles previously equipped items. When unequipping, it validates inventory space and
    // moves items back to inventory. Equipment changes are broadcast to clients for visibility.
    public override void Update(in NTT ntt, ref InventoryComponent inv, ref EquipmentComponent eq, ref RequestChangeEquipComponent change)
    {
        ref var item = ref NttWorld.GetEntity(change.ItemNetId);

        if (change.Equip)
        {
            // === EQUIP ITEM TO SLOT ===
            // Handle previously equipped item in this slot
            var previouslyEquipped = eq.Items[change.Slot];
            if (previouslyEquipped != default)
            {
                // Move previously equipped item back to inventory
                InventoryHelper.AddItem(ntt, ref inv, previouslyEquipped, netSync: true);
            }

            // Handle bow to non-bow weapon transition
            if (change.Slot == MsgItemPosition.RightWeapon)
            {
                var currentWeapon = eq.Items[MsgItemPosition.RightWeapon];
                var equippedArrows = eq.Items[MsgItemPosition.LeftWeapon];

                if (currentWeapon != default && equippedArrows != default)
                {
                    var currentWeaponItem = currentWeapon.Get<ItemComponent>();
                    var newWeaponItem = item.Get<ItemComponent>();

                    // If switching from bow to non-bow, unequip arrows
                    if (ItemHelper.IsBow(ref currentWeaponItem) && !ItemHelper.IsBow(ref newWeaponItem))
                    {
                        eq.Items[MsgItemPosition.LeftWeapon] = default;
                        InventoryHelper.AddItem(ntt, ref inv, equippedArrows, netSync: true);

                        if (IsLogging)
                            FConsole.WriteLine("{ntt} auto-unequipped arrows when switching from bow to {item}", ntt, item);
                    }
                }
            }

            // Place new item in equipment slot
            eq.Items[change.Slot] = item;


            // Remove item from inventory and broadcast equipment change
            InventoryHelper.RemoveNttFromInventory(ntt, ref inv, item, netSync: true);
            var equipPacket = MsgItem.Create(item.Id, item.Id, (int)change.Slot, MsgItemType.SetEquipPosition);
            ntt.NetSync(ref equipPacket, broadcast: true);


            if (IsLogging)
                FConsole.WriteLine("{ntt} equipped {item} to slot {slot}", ntt, item, change.Slot);
        }
        else
        {
            // === UNEQUIP ITEM FROM SLOT ===
            // Validate inventory has space for unequipped item
            if (!InventoryHelper.HasFreeSpace(ref inv))
            {
                if (IsLogging)
                    FConsole.WriteLine("{ntt} has no inventory space to unequip {item}", ntt, item);

                ntt.Remove<RequestChangeEquipComponent>();
                return;
            }

            // Remove from equipment slot and add to inventory
            eq.Items[change.Slot] = default;
            InventoryHelper.AddItem(ntt, ref inv, in item, netSync: true);

            if (IsLogging)
                FConsole.WriteLine("{ntt} unequipped {item} from slot {slot}", ntt, item, change.Slot);
        }

        // Clean up the equipment change request
        ntt.Remove<RequestChangeEquipComponent>();
    }
}