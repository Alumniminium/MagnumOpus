using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Systems
{
    public sealed class EquipSystem : NttSystem<InventoryComponent, EquipmentComponent, RequestChangeEquipComponent>
    {
        public EquipSystem() : base("Equip", threads: 2) { }

        public override void Update(in NTT ntt, ref InventoryComponent inv, ref EquipmentComponent eq, ref RequestChangeEquipComponent change)
        {
            ref var item = ref NttWorld.GetEntity(change.ItemNetId);

            if (change.Equip)
            {
                // TODO: If current weapon is bow and new weapon is not bow, unequip arrows
                var oldEq = eq.Items[change.Slot];

                if (oldEq != default)
                    InventoryHelper.AddItem(ntt, ref inv, oldEq, netSync: true);

                eq.Items[change.Slot] = item;

                var msg = MsgItem.Create(item.Id, item.Id, (int)change.Slot, MsgItemType.SetEquipPosition);
                ntt.NetSync(ref msg);
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