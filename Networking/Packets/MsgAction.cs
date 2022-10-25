using System.Runtime.InteropServices;
using MagnumOpus.Enums;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public unsafe struct MsgAction
    {
        [FieldOffset(0)]
        public ushort Size;
        [FieldOffset(2)]
        public ushort Id;
        [FieldOffset(4)]
        public int Timestamp;
        [FieldOffset(8)]
        public int UniqueId;
        [FieldOffset(12)]
        public int Param;
        [FieldOffset(16)]
        public int Param2;
        [FieldOffset(16)]
        public ushort X;
        [FieldOffset(18)]
        public ushort Y;
        [FieldOffset(20)]
        public ushort Direction;
        [FieldOffset(22)]
        public MsgActionType Type;

        public static Memory<byte> Create(int timestamp, int uniqueId, int param, ushort x, ushort y, ushort direction, MsgActionType type)
        {
            MsgAction msgP = new()
            {
                Size = (ushort)sizeof(MsgAction),
                Id = 1010,
                Timestamp = timestamp,
                UniqueId = uniqueId,
                Param = param,
                X = x,
                Y = y,
                Direction = direction,
                Type = type
            };
            return msgP;
        }

        public static implicit operator Memory<byte>(MsgAction msg)
        {
            var buffer = new byte[sizeof(MsgAction) + 8];
            fixed (byte* p = buffer)
                *(MsgAction*)p = *&msg;

            var tqserver = "TQServer".ToCharArray();
            for (var i = 0; i < 8; i++)
                buffer[buffer.Length - 8 + i] = (byte)tqserver[i];
            return buffer;
        }
        public static implicit operator MsgAction(Memory<byte> buffer)
        {
            fixed (byte* p = buffer.Span)
                return *(MsgAction*)p;
        }
    }
}