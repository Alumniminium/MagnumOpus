using System.Buffers;
using System.Runtime.InteropServices;
using MagnumOpus.Enums;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgItem
    {
        private ushort Size;
        private ushort Id;
        private int UnqiueId;
        private int Param;
        private MsgItemType Type;
        private int Timestamp;
        private int Value;

        public static MsgItem Create(int uid, int value, int param, MsgItemType type)
        {
            var packet = stackalloc MsgItem[1];
            {
                packet->Size = (ushort)sizeof(MsgItem);
                packet->Id = 1009;
                packet->UnqiueId = uid;
                packet->Param = param;
                packet->Type = type;
                packet->Value = value;
                packet->Timestamp = Environment.TickCount;
            }

            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(MsgUpdate));
            fixed (byte* p = buffer)
                *(MsgItem*)p = *packet;
            return buffer;
        }

        public static implicit operator byte[](MsgItem msg)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(MsgUpdate));
            fixed (byte* p = buffer)
                *(MsgItem*)p = *&msg;
            return buffer;
        }

        public static implicit operator MsgItem(byte[] msg)
        {
            fixed (byte* p = msg)
                return *(MsgItem*)p;
        }
    }
}