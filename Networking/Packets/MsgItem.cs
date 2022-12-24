using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using MagnumOpus.ECS;
using MagnumOpus.Enums;

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
        public int Timestamp;
        public int Value;

        public static MsgItem Create(int uid, int value, int param, int timestamp, MsgItemType type)
        {
            var msg = new MsgItem
            {
                Size = (ushort)sizeof(MsgItem),
                Id = 1009,
                UnqiueId = uid,
                Param = param,
                Type = type,
                Value = value,
                Timestamp = timestamp,
            };
            return msg;
        }

        [PacketHandler(PacketId.MsgItem)]
        public static void Process(PixelEntity ntt, Memory<byte> memory)
        {
            var msg = Co2Packet.Deserialze<MsgItem>(in memory);

            switch (msg.Type)
            {
                case MsgItemType.BuyItemAddItem:
                    break;
                case MsgItemType.SellItem:
                    break;
                case MsgItemType.RemoveInventory:
                    break;
                case MsgItemType.EquipItem:
                    break;
                case MsgItemType.SetEquipPosition:
                    break;
                case MsgItemType.UnEquipItem:
                    break;
                case MsgItemType.ShowWarehouseMoney:
                    break;
                case MsgItemType.DepositWarehouseMoney:
                    break;
                case MsgItemType.WithdrawWarehouseMoney:
                    break;
                case MsgItemType.DropGold:
                    break;
                case MsgItemType.RepairItem:
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
                case MsgItemType.Ping:
                    var reply = MsgItem.Create(ntt.NetId, msg.Value, msg.Param, msg.Timestamp, MsgItemType.Ping);
                    // var tick = MsgTick.Create(in ntt);
                    ntt.NetSync(ref reply);
                    // ntt.NetSync(ref tick);
                    break;
                case MsgItemType.Enchant:
                    break;
                case MsgItemType.BoothAddCp:
                    break;
            }
        }
    }
}