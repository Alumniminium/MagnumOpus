using System.Runtime.InteropServices;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgItem
    {
        public ushort Size;
        public ushort Id;
        public int UnqiueId;
        public int Param;
        public MsgItemType Type;
        public uint Timestamp;
        public int Value;

        public readonly int Money => UnqiueId;
        public readonly int ItemUniqueId => UnqiueId;
        public readonly int ItemSlot => Param;
        public readonly int ShopId => UnqiueId;
        public readonly int ShopItemId => Param;

        public static MsgItem Create(int uid, int value, int param, MsgItemType type)
        {
            var msg = new MsgItem
            {
                Size = (ushort)sizeof(MsgItem),
                Id = 1009,
                UnqiueId = uid,
                Param = param,
                Type = type,
                Value = value,
                Timestamp = (uint)NttWorld.Tick,
            };
            return msg;
        }

        [PacketHandler(PacketId.MsgItem)]
        public static void Process(NTT ntt, Memory<byte> memory)
        {
            var msg = Co2Packet.Deserialize<MsgItem>(memory.Span);

            switch (msg.Type)
            {
                case MsgItemType.Ping:
                    {
                        var tick = MsgTick.Create(in ntt);
                        ntt.NetSync(ref tick);
                        ntt.NetSync(ref msg);
                        break;
                    }
                case MsgItemType.DropMoney:
                    {
                        var rdmc = new RequestDropMoneyComponent(msg.Money);
                        ntt.Set(ref rdmc);

                        ntt.NetSync(ref msg);
                        break;
                    }
                case MsgItemType.RemoveInventory:
                    {
                        ref readonly var itemNtt = ref NttWorld.GetEntity(msg.ItemUniqueId);
                        var drc = new RequestDropItemComponent(in itemNtt);
                        ntt.Set(ref drc);

                        ntt.NetSync(ref msg);
                        break;
                    }
                case MsgItemType.Use:
                case MsgItemType.UnEquip:
                    {
                        var slot = msg.ItemSlot;
                        ref readonly var itemNtt = ref NttWorld.GetEntity(msg.ItemUniqueId);
                        ref var item = ref itemNtt.Get<ItemComponent>();

                        var isArrow = ItemHelper.IsArrow(ref item);
                        slot = isArrow ? 5 : slot;

                        if (slot == 0 && !isArrow)
                        {
                            var uic = new RequestItemUseComponent(msg.ItemUniqueId, slot);
                            ntt.Set(ref uic);
                        }
                        else
                        {
                            var rue = new RequestChangeEquipComponent(msg.ItemUniqueId, slot, msg.Type == MsgItemType.Use);
                            ntt.Set(ref rue);
                        }
                        ntt.NetSync(ref msg);
                        break;
                    }
                case MsgItemType.Sell:
                case MsgItemType.Buy:
                    {
                        var rbi = new RequestShopItemTransactionComponent(msg.ShopId, msg.ShopItemId, msg.Type == MsgItemType.Buy);
                        ntt.Set(ref rbi);
                        ntt.NetSync(ref msg);
                        break;
                    }

                case MsgItemType.SetEquipPosition:
                    break;
                case MsgItemType.ShowWarehouseMoney:
                    break;
                case MsgItemType.DepositWarehouseMoney:
                    break;
                case MsgItemType.WithdrawWarehouseMoney:
                    break;
                case MsgItemType.Repair:
                    break;
                case MsgItemType.UpdateDurability:
                    break;
                case MsgItemType.RemoveEquipment:
                    break;
                case MsgItemType.UpgradeDragonball:
                    break;
                case MsgItemType.UpgradeMeteor:
                    break;
                case MsgItemType.ShowVendingList:
                    break;
                case MsgItemType.AddVendingItem:
                    break;
                case MsgItemType.RemoveVendingItem:
                    break;
                case MsgItemType.BuyVendingItem:
                    break;
                case MsgItemType.UpdateArrowCount:
                    break;
                case MsgItemType.ParticleEffect:
                    break;
                case MsgItemType.Enchant:
                    break;
                case MsgItemType.BoothAddCp:
                    break;
                default:
                    FConsole.WriteLine($"Unhandled MsgItem type: {msg.Type}");
                    FConsole.WriteLine(memory.Dump());
                    break;
            }
        }
    }
}