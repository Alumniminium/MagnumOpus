using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using MagnumOpus.ECS;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgTick
    {
        public ushort Size;
        public ushort Id;
        public int UniqueId;
        public int Timestamp;
        public uint Junk1;
        public uint Junk2;
        public uint Junk3;
        public uint Junk4;
        public uint Hash;

        public static Memory<byte> Create(in PixelEntity target)
        {
            var packet = stackalloc MsgTick[1];
            packet->Size = (ushort)sizeof(MsgTick);
            packet->Id = 1012;
            packet->UniqueId = target.Id;
            packet->Timestamp = Environment.TickCount + 100000;
            packet->Junk1 = 0;
            packet->Junk2 = 0;
            packet->Junk3 = 0;
            packet->Junk4 = 0;
            packet->Hash = 0;

            var buffer = new byte[sizeof(MsgTick)];
            fixed (byte* p = buffer)
                *(MsgTick*)p = *packet;
            return buffer;
        }

        private static uint HashName(string name)
        {
            if (string.IsNullOrEmpty(name) || name.Length < 4)
                return 0x9D4B5703;

            var buffer = Encoding.GetEncoding("iso-8859-1").GetBytes(name);
            fixed (byte* pBuf = buffer)
                return ((ushort*)pBuf)[0] ^ 0x9823U;
        }

        public static implicit operator Memory<byte>(MsgTick msg)
        {
            var buffer = new byte[sizeof(MsgTick)];
            fixed (byte* p = buffer)
                *(MsgTick*)p = *&msg;
            return buffer;
        }
    }
}