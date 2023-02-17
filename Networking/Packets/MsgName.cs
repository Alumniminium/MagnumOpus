using System.Buffers;
using System.Runtime.InteropServices;
using MagnumOpus.Enums;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public unsafe struct MsgName
    {
        [FieldOffset(0)]
        public ushort Size;
        [FieldOffset(2)]
        public ushort Id;
        [FieldOffset(4)]
        public int UniqueId;
        [FieldOffset(4)]
        public int X;
        [FieldOffset(6)]
        public int Y;
        [FieldOffset(8)]
        public MsgNameType Type;
        [FieldOffset(9)]
        public byte Count;
        [FieldOffset(10)]
        public byte Length;
        [FieldOffset(11)]
        public fixed byte Params[255];

        public static MsgName Create(int netId, string param, byte action)
        {
            var msg = new MsgName
            {
                Size = (ushort)(13 + param.Length),
                Id = 1015,
                UniqueId = netId,
                Type = (MsgNameType)action,
                Count = 1,
                Length = (byte)param.Length
            };
                for (byte i = 0; i < (byte)param.Length; i++)
                msg.Params[i] = (byte)param[i];
            return msg;
        }

        public static MsgName Create(ushort x, ushort y, string param, MsgNameType action)
        {
            var msg = new MsgName
            {
                Size = (ushort)(13 + param.Length),
                Id = 1015,
                X = x,
                Y = y,
                Type = action,
                Count = 1,
                Length = (byte)param.Length
            };
                for (byte i = 0; i < (byte)param.Length; i++)
                msg.Params[i] = (byte)param[i];
            return msg;
        }
    }
}