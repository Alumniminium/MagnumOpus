using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;
using NttECS.ECS;

namespace MagnumOpus.Systems;

public sealed class EquipSystem : NttSystem<InventoryComponent, EquipmentComponent, RequestChangeEquipComponent>
{
    public EquipSystem() : base("Equip", threads: 1, log: true) { }

    // Handles equipment changes including equipping and unequipping items between inventory and
    // equipment slots. When equipping, the system moves items from inventory to equipment slots
    // and handles previously equipped items. When unequipping, it validates inventory space and
    // moves items back to inventory. Equipment changes are broadcast to clients for visibility.
    public override void Update(in NTT ntt, ref InventoryComponent inv, ref EquipmentComponent eq, ref RequestChangeEquipComponent change)
    {
        ref readonly var newItemNtt = ref NttWorld.GetEntity(change.ItemNetId);
        ref readonly var newItemC = ref newItemNtt.Get<ItemComponent>();

        if (change.Equip)
        {
            // === EQUIP ITEM TO SLOT ===
            // Handle previously equipped item in this slot
            var oldItemNtt = eq.Items[change.Slot];
            if (oldItemNtt != default)
            {
                // Move previously equipped item back to inventory
                InventoryHelper.AddItem(ntt, ref inv, oldItemNtt, netSync: true);
            }

            // // Handle bow to non-bow weapon transition
            // if (change.Slot == MsgItemPosition.PrimaryWeapon)
            // {
            //     var currentWeapon = eq.Items[MsgItemPosition.PrimaryWeapon];
            //     var currentArrows = eq.Items[MsgItemPosition.SecondaryWeapon];

            //     if (currentWeapon != default && currentArrows != default)
            //     {
            //         ref readonly var currentWeaponItem = ref currentWeapon.Get<ItemComponent>();

            //         // If switching from bow to non-bow, unequip arrows
            //         if (ItemHelper.IsBow(in currentWeaponItem) && !ItemHelper.IsBow(in newItemC))
            //         {
            //             eq.Items[MsgItemPosition.SecondaryWeapon] = default;
            //             InventoryHelper.AddItem(ntt, ref inv, currentArrows, netSync: true);

            //             if (IsLogging)
            //                 FConsole.WriteLine("{ntt} auto-unequipped arrows when switching from bow to {item}", ntt, newItemNtt);
            //         }
            //     }
            // }

            // Place new item in equipment slot
            eq.Items[change.Slot] = newItemNtt;
            // Remove item from inventory and broadcast equipment change
            InventoryHelper.RemoveNttFromInventory(ntt, ref inv, newItemNtt, netSync: true);


            var msg = MsgItem.EquipItem(newItemNtt, change.Slot);
            ntt.NetSync(ref msg);

            var itemInfoMsg = MsgItemInformation.Create(newItemNtt, MsgItemInfoAction.AddItem, change.Slot);
            ntt.NetSync(ref itemInfoMsg);

            if (IsLogging)
                FConsole.WriteLine("{ntt} equipped {item} to slot {slot}", ntt, newItemNtt, change.Slot);
        }
        else
        {
            // === UNEQUIP ITEM FROM SLOT ===
            // Validate inventory has space for unequipped item
            if (!InventoryHelper.HasFreeSpace(ref inv))
            {
                if (IsLogging)
                    FConsole.WriteLine("{ntt} has no inventory space to unequip {item}", ntt, newItemNtt);

                ntt.Remove<RequestChangeEquipComponent>();
                return;
            }

            // Remove from equipment slot and add to inventory
            eq.Items[change.Slot] = default;
            InventoryHelper.AddItem(ntt, ref inv, in newItemNtt, netSync: true);

            if (IsLogging)
                FConsole.WriteLine("{ntt} unequipped {item} from slot {slot}", ntt, newItemNtt, change.Slot);
        }

        // Clean up the equipment change request
        ntt.Remove<RequestChangeEquipComponent>();
    }
}