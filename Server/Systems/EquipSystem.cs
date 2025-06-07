using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;
using NttECS.ECS;

namespace MagnumOpus.Systems
{
    /// <summary>
    /// Handles equipment changes including equipping and unequipping items between inventory and equipment slots.
    /// Manages item transfers, slot validation, and inventory space requirements for equipment operations.
    /// </summary>
    public sealed class EquipSystem : NttSystem<InventoryComponent, EquipmentComponent, RequestChangeEquipComponent>
    {
        /// <summary>
        /// Initializes the EquipSystem with limited threading for equipment processing.
        /// </summary>
        public EquipSystem() : base("Equip", threads: 1, log: false) { }

        /// <summary>
        /// Processes equipment change requests, handling both equipping and unequipping operations.
        /// </summary>
        /// <param name="ntt">The entity changing equipment</param>
        /// <param name="inv">Inventory component for item storage</param>
        /// <param name="eq">Equipment component containing equipped items</param>
        /// <param name="change">Request change equip component specifying the operation</param>
        public override void Update(in NTT ntt, ref InventoryComponent inv, ref EquipmentComponent eq, ref RequestChangeEquipComponent change)
        {
            ref var item = ref NttWorld.GetEntity(change.ItemNetId);

            if (change.Equip)
            {
                // TODO: If current weapon is bow and new weapon is not bow, unequip arrows
                var previouslyEquipped = eq.Items[change.Slot];

                if (previouslyEquipped != default)
                    InventoryHelper.AddItem(ntt, ref inv, previouslyEquipped, netSync: true);

                eq.Items[change.Slot] = item;

                var equipPacket = MsgItem.Create(item.Id, item.Id, (int)change.Slot, MsgItemType.SetEquipPosition);
                ntt.NetSync(ref equipPacket);
                InventoryHelper.RemoveNttFromInventory(ntt, ref inv, item, netSync: true);

                if (IsLogging)
                    FConsole.WriteLine("{ntt} equipped {item} to {slot}", ntt, item, change.Slot);
            }
            else
            {
                if (!InventoryHelper.HasFreeSpace(ref inv))
                {
                    FConsole.WriteLine("{ntt} has no free space in inventory to unequip {item}", ntt, item);
                    ntt.Remove<RequestChangeEquipComponent>();
                    return;
                }

                eq.Items[change.Slot] = default;
                InventoryHelper.AddItem(ntt, ref inv, in item, true);
                if (IsLogging)
                    FConsole.WriteLine("{ntt} unequipped {item} from {slot}", ntt, item, change.Slot);
            }
            ntt.Remove<RequestChangeEquipComponent>();
        }
    }
}