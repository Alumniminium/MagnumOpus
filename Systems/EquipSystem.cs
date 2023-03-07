using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class EquipSystem : NttSystem<InventoryComponent, EquipmentComponent, RequestChangeEquipComponent>
    {
        public EquipSystem() : base("Equip", threads: 2) { }

        public override void Update(in NTT ntt, ref InventoryComponent inv, ref EquipmentComponent eq, ref RequestChangeEquipComponent change)
        {
            ref var item = ref NttWorld.GetEntityByNetId(change.ItemNetId);

            if (change.Equip)
            {
                // TODO: If current weapon is bow and new weapon is not bow, unequip arrows
                var oldEq = eq.Items[change.Slot];

                if(oldEq != default)
                    InventoryHelper.AddItem(in ntt, ref inv, in oldEq, netSync: true);
                
                InventoryHelper.RemoveNttFromInventory(in ntt, ref inv, in item, netSync: true);

                eq.Items[change.Slot] = item;

                var msg = MsgItem.Create(item.NetId, 0, (int)change.Slot, MsgItemType.SetEquipPosition);
                ntt.NetSync(ref msg);

                if (Trace) 
                    Logger.Debug("{ntt} equipped {item} to {slot}", ntt, item, change.Slot);
            }
            else
            {
                if (!InventoryHelper.HasFreeSpace(ref inv))
                {
                    Logger.Error("{ntt} has no free space in inventory to unequip {item}", ntt, item);
                    ntt.Remove<RequestChangeEquipComponent>();
                    return;
                }
                
                eq.Items[change.Slot] = default;
                InventoryHelper.AddItem(in ntt, ref inv, in item, true);
                if (Trace)
                    Logger.Debug("{ntt} unequipped {item} from {slot}", ntt, item, change.Slot);
            }
            ntt.Remove<RequestChangeEquipComponent>();
        }
    }
}