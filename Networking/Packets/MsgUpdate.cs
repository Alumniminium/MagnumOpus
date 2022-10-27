using System.Buffers;
using System.Runtime.InteropServices;
using MagnumOpus.Enums;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct MsgUserAttrib
    {
        public ushort Size;
        public ushort Id;
        public int UniqueId;
        public int Amount;
        public MsgUserAttribType Type;
        public ulong Value;

        public static implicit operator Memory<byte>(MsgUserAttrib msg)
        {
            var buffer = new byte[sizeof(MsgUserAttrib)];
            fixed (byte* p = buffer)
                *(MsgUserAttrib*)p = *&msg;
            return buffer;
        }

        public static Memory<byte> Create(int entityId, ulong value, MsgUserAttribType type)
        {
            var packet = stackalloc MsgUserAttrib[1];
            packet->Size = (ushort)(sizeof(MsgUserAttrib));
            packet->Id = 1017;
            packet->UniqueId = entityId;
            packet->Amount = 1;
            packet->Type = type;
            packet->Value = value;
    
            var buffer = new byte[packet->Size];
            fixed (byte* p = buffer)
                *(MsgUserAttrib*)p = *packet;
            return buffer;
        }
    }
}