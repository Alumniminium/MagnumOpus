using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
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

        public static Memory<byte> Create(int uid, int value, int param, int timestamp, MsgItemType type)
        {
            var packet = stackalloc MsgItem[1];
            {
                packet->Size = (ushort)sizeof(MsgItem);
                packet->Id = 1009;
                packet->UnqiueId = uid;
                packet->Param = param;
                packet->Type = type;
                packet->Value = value;
                packet->Timestamp = timestamp;
            }

            var buffer = new byte[sizeof(MsgItem)];
            fixed (byte* p = buffer)
                *(MsgItem*)p = *packet;
            return buffer;
        }

        public static implicit operator Memory<byte>(MsgItem msg)
        {
            var buffer = new byte[sizeof(MsgItem)];
            fixed (byte* p = buffer)
                *(MsgItem*)p = *&msg;
            return buffer;
        }

        public static implicit operator MsgItem(in Memory<byte> msg)
        {
            fixed (byte* p = msg.Span)
                return *(MsgItem*)p;
        }
    }
}