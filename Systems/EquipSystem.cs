using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class EquipSystem : NttSystem<InventoryComponent, EquipmentComponent, RequestChangeEquipComponent>
    {
        public EquipSystem() : base("Equip", threads: 1) { }

        public override void Update(in NTT ntt, ref InventoryComponent inv, ref EquipmentComponent eq, ref RequestChangeEquipComponent change)
        {
            ref var item = ref NttWorld.GetEntityByNetId(change.ItemNetId);

            if (change.Equip)
            {
                // TODO: If current weapon is bow and new weapon is not bow, unequip arrows

                var newEqInvIdx = Array.IndexOf(inv.Items, item);
                var oldEq = eq.Items[change.Slot];
                inv.Items[newEqInvIdx] = oldEq;
                eq.Items[change.Slot] = item;

                var msg = MsgItem.Create(item.NetId, 0, (int)change.Slot, MsgItemType.SetEquipPosition);
                ntt.NetSync(ref msg);
                var remInv = MsgItem.Create(item.NetId, item.NetId, item.NetId, MsgItemType.RemoveInventory);
                ntt.NetSync(ref remInv);
            }
            else
            {
                eq.Items[change.Slot] = default;
                var emptInvSlotIdx = Array.IndexOf(inv.Items, default);
                inv.Items[emptInvSlotIdx] = item;

                var msgAddInv = MsgItemInformation.Create(in item);
                ntt.NetSync(ref msgAddInv);
            }
            ntt.Remove<RequestChangeEquipComponent>();
        }
    }
}