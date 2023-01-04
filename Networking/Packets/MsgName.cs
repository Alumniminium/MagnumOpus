using System.Buffers;
using MagnumOpus.Enums;

namespace MagnumOpus.Networking.Packets
{
    public unsafe struct MsgName
    {
        public ushort Size;
        public ushort Id;
        public int Data;
        public MsgNameType Type;
        public byte Count;
        public fixed byte Params[255];

        public static Memory<byte> Create(int data, string param, byte action)
        {
            var Out = new byte[13 + param.Length];
            fixed (byte* p = Out)
            {
                *(short*)(p + 0) = (short)Out.Length;
                *(short*)(p + 2) = 1015;
                *(int*)(p + 4) = data;
                *(p + 8) = action;
                *(p + 9) = 0x01;
                *(p + 10) = (byte)param.Length;
                for (byte i = 0; i < (byte)param.Length; i++)
                    *(p + 11 + i) = (byte)param[i];
            }
            return Out;
        }

        public static Memory<byte> Create(ushort x, ushort y, string param, MsgNameType action)
        {
            var Out = new byte[13 + param.Length];
            fixed (byte* p = Out)
            {
                *(short*)(p + 0) = (short)Out.Length;
                *(short*)(p + 2) = 1015;
                *(ushort*)(p + 4) = x;
                *(ushort*)(p + 6) = y;
                *(p + 8) = (byte)action;
                *(p + 9) = 0x01;
                *(p + 10) = (byte)param.Length;
                for (byte i = 0; i < (byte)param.Length; i++)
                    *(p + 11 + i) = (byte)param[i];
            }
            return Out;
        }
    }
}