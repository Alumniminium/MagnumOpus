using System.Buffers;
using System.Runtime.InteropServices;
using MagnumOpus.Enums;

namespace MagnumOpus.Networking.Packets
{
    public readonly struct MsgUserAttribValues
    {
        public readonly MsgUserAttribType Type;
        public readonly ulong Value;

        public MsgUserAttribValues(ulong value, MsgUserAttribType type)
        {
            Type = type;
            Value = value;
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct MsgUserAttrib
    {
        public ushort Size;
        public ushort Id;
        public int UniqueId;
        public int Amount;
        public fixed byte Values[24];

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
            packet->Size = (ushort)(sizeof(MsgUserAttrib)-12);
            packet->Id = 1017;
            packet->UniqueId = entityId;
            packet->Amount = 1;
            *(MsgUserAttribType*)packet->Values[12] = type;
            *(ulong*)packet->Values[16] = value;
    
            var buffer = new byte[packet->Size];
            fixed (byte* p = buffer)
                *(MsgUserAttrib*)p = *packet;
            return buffer;
        }
        public static Memory<byte> Create(int entityId, in MsgUserAttribValues[] values)
        {
            var packet = stackalloc MsgUserAttrib[1];
            packet->Size = (ushort)(sizeof(MsgUserAttrib) -12 + values.Length * 12);
            packet->Id = 1017;
            packet->UniqueId = entityId;
            packet->Amount = values.Length;

            fixed(MsgUserAttribValues* v = values)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    *(MsgUserAttribType*)packet->Values[i * 12] = v[i].Type;
                    *(ulong*)(packet->Values + i * 12 + 4) = v[i].Value;
                }
            }

            var buffer = new byte[packet->Size];
            fixed (byte* p = buffer)
                *(MsgUserAttrib*)p = *packet;
            return buffer;
        }
    }
}