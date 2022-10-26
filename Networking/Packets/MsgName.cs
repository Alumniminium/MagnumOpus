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

        public static Memory<byte> Create(int data, string[] Params, MsgNameType action)
        {
            var strLength = 0;
            for (var i = 0; i < Params.Length; i++)
            {
                strLength += Params[i].Length + 1;
            }

            var Out = new byte[12 + strLength];
            fixed (byte* p = Out)
            {
                *(short*)(p + 0) = (short)Out.Length;
                *(short*)(p + 2) = 1015;
                *(int*)(p + 4) = data;
                *(p + 8) = (byte)action;
                *(p + 9) = (byte)Params.Length;

                var pos = 10;
                for (var x = 0; x < Params.Length; x++)
                {
                    *(p + pos) = (byte)Params[x].Length;
                    for (byte i = 0; i < (byte)Params[x].Length; i++)
                        *(p + pos + 1 + i) = (byte)Params[x][i];
                    pos += Params[x].Length + 1;
                }
            }
            return Out;
        }

        public static implicit operator Memory<byte>(MsgName msg)
        {
            var buffer = new byte[sizeof(MsgName)];
            fixed (byte* p = buffer)
                *(MsgName*)p = *&msg;
            return buffer;
        }
    }
}