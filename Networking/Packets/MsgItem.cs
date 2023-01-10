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
                    var tick = MsgTick.Create(in ntt);
                    ntt.NetSync(ref tick);
                    ntt.NetSync(memory[..sizeof(MsgItem)]);
                    break;

                case MsgItemType.DropMoney:
                    var rdmc = new RequestDropMoneyComponent(ntt.Id, msg.Value);
                    ntt.Set(ref rdmc);

                    ntt.NetSync(memory[..sizeof(MsgItem)]);
                    break;
                
                case MsgItemType.RemoveInventory:
                        var drc = new RequestDropItemComponent(ntt.Id, msg.UnqiueId);
                        ntt.Set(ref drc);

                        ntt.NetSync(memory[..sizeof(MsgItem)]);
                        break;
                
                case MsgItemType.Use:
                case MsgItemType.UnEquip:
                        var itemNttId = msg.UnqiueId;
                        var slot = msg.Param;

                        if (slot == 0)
                        {
                            var uic = new RequestItemUseComponent(ntt.Id, itemNttId, slot);
                            ntt.Set(ref uic);
                        }
                        else
                        {
                            var rue = new RequestChangeEquipComponent(ntt.Id, itemNttId, slot, msg.Type == MsgItemType.Use);
                            ntt.Set(ref rue);
                        }
                        ntt.NetSync(memory[..sizeof(MsgItem)]);
                        break;
                    
                case MsgItemType.Sell:
                case MsgItemType.Buy:
                        var shopId = msg.UnqiueId;
                        var itemId = msg.Param;

                        var rbi = new RequestShopItemTransactionComponent(ntt.Id, shopId, itemId, msg.Type == MsgItemType.Buy);
                        ntt.Set(ref rbi);

                        ntt.NetSync(memory[..sizeof(MsgItem)]);
                        break;
                    
                default:
                    FConsole.WriteLine($"Unhandled MsgItem type: {msg.Type}");
                    FConsole.WriteLine(memory.Dump());
                    break;
            }
        }
    }
}