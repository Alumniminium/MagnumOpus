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
                switch (change.Slot)
                {
                    case MsgItemPosition.Armor:
                            eq.Armor = item;
                            break;
                    case MsgItemPosition.RightWeapon:
                            eq.MainHand = item;
                            break;
                    case MsgItemPosition.LeftWeapon:
                            eq.OffHand = item;
                            break;
                    case MsgItemPosition.Ring:
                            eq.Ring = item;
                            break;
                    case MsgItemPosition.Necklace:
                            eq.Necklace = item;
                            break;
                    case MsgItemPosition.Head:
                            eq.Head = item;
                            break;
                    case MsgItemPosition.Boots:
                            eq.Boots = item;
                            break;
                }

                var msg = MsgItem.Create(item.NetId, 0, (int)change.Slot, PixelWorld.Tick, MsgItemType.SetEquipPosition);
                ntt.NetSync(ref msg);
            }
            else
            {
                for(int i = 0; i<inv.Items.Length; i++)
                {
                    if (inv.Items[i] == default)
                    {
                        switch (change.Slot)
                        {
                            case MsgItemPosition.Armor:
                                eq.Armor = default;
                                break;
                            case MsgItemPosition.RightWeapon:
                                eq.MainHand = default;
                                break;
                            case MsgItemPosition.LeftWeapon:
                                eq.OffHand = default;
                                break;
                            case MsgItemPosition.Ring:
                                eq.Ring = default;
                                break;
                            case MsgItemPosition.Necklace:
                                eq.Necklace = default;
                                break;
                            case MsgItemPosition.Head:
                                eq.Head = default;
                                break;
                            case MsgItemPosition.Boots:
                                eq.Boots = default;
                                break;
                        }
                        
                        inv.Items[i] = item;
                        
                        var msgAddInv = MsgItemInformation.Create(in item);
                        ntt.NetSync(ref msgAddInv);

                        var msg = MsgItem.Create(item.NetId, 0, 0, PixelWorld.Tick, MsgItemType.BuyItemAddItem);
                        ntt.NetSync(ref msg);
                        break;
                    }
                }
            }
            ntt.Remove<RequestChangeEquipComponent>();
        }
    }
}