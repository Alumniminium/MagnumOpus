using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
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

        public static MsgItem Create(int uid, int value, int param, uint timestamp, MsgItemType type)
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
                case MsgItemType.Ping:
                    var reply = MsgItem.Create(ntt.NetId, msg.Value, msg.Param, msg.Timestamp, MsgItemType.Ping);
                    // var tick = MsgTick.Create(in ntt);
                    ntt.NetSync(ref reply);
                    // ntt.NetSync(ref tick);
                    break;
                case MsgItemType.RemoveInventory:
                    {
                        var drc = new DropRequestComponent(ntt.Id, msg.UnqiueId);
                        ntt.Add(ref drc);

                        ntt.NetSync(memory[..sizeof(MsgItem)]);
                        break;
                    }
                case MsgItemType.UseItem:
                case MsgItemType.UnEquipItem:
                    {
                        var itemNttId = msg.UnqiueId;
                        var slot = msg.Param;

                        if (slot == 0)
                        {
                            var uic = new RequestItemUseComponent(ntt.Id, itemNttId, slot);
                            ntt.Add(ref uic);
                        }
                        else
                        {
                            var rue = new RequestChangeEquipComponent(ntt.Id, itemNttId, slot, msg.Type == MsgItemType.UseItem);
                            ntt.Add(ref rue);
                        }
                        ntt.NetSync(memory[..sizeof(MsgItem)]);
                        break;
                    }
                case MsgItemType.SellItem:
                case MsgItemType.BuyItemAddItem:
                    {
                        var shopId = msg.UnqiueId;
                        var itemId = msg.Param;

                        var rbi = new RequestShopItemTransactionComponent(ntt.Id, shopId, itemId, msg.Type == MsgItemType.BuyItemAddItem);
                        ntt.Add(ref rbi);

                        ntt.NetSync(memory[..sizeof(MsgItem)]);
                        break;
                    }
                default:
                    FConsole.WriteLine($"Unhandled MsgItem type: {msg.Type}");
                    FConsole.WriteLine(memory.Dump());
                    break;
            }
        }
    }
}