using System.Buffers;
using System.Runtime.InteropServices;
using MagnumOpus.Enums;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct MsgUpdate
    {
        public ushort Size;
        public ushort Id;
        public int UniqueId;
        public int Amount;
        public MsgUpdateType Type;
        public long Value;

        public static implicit operator Memory<byte>(MsgUpdate msg)
        {
            var buffer = new byte[sizeof(MsgUpdate)];
            fixed (byte* p = buffer)
                *(MsgUpdate*)p = *&msg;
            return buffer;
        }

        public static Memory<byte> Create(int entityId, long value, MsgUpdateType type)
        {
            var packet = stackalloc MsgUpdate[1];
            packet->Size = (ushort)sizeof(MsgUpdate);
            packet->Id = 1017;
            packet->UniqueId = entityId;
            packet->Amount = 1;
            packet->Value = value;
            packet->Type = type;
            var buffer = new byte[sizeof(MsgUpdate)];
            fixed (byte* p = buffer)
                *(MsgUpdate*)p = *packet;
            return buffer;
        }
    }
}