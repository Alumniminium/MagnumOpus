using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class EquipSystem : PixelSystem<InventoryComponent, EquipmentComponent, RequestChangeEquipComponent>
    {
        public EquipSystem() : base("Equip System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref InventoryComponent inv, ref EquipmentComponent eq, ref RequestChangeEquipComponent change)
        {
            ref var item = ref PixelWorld.GetEntityByNetId(change.ItemNetId);

            if (change.Equip)
            {
                var newEqInvIdx = Array.IndexOf(inv.Items, item);
                var oldEq = eq.Items[change.Slot];
                inv.Items[newEqInvIdx] = oldEq;
                eq.Items[change.Slot] = item;

                var msg = MsgItem.Create(item.NetId, 0, (int)change.Slot, PixelWorld.Tick, MsgItemType.SetEquipPosition);
                ntt.NetSync(ref msg);
                var remInv = MsgItem.Create(item.NetId, item.NetId, item.NetId, PixelWorld.Tick, MsgItemType.RemoveInventory);
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