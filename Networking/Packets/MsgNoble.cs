using System.Buffers;
using System.Runtime.InteropServices;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgNoble
    {
        public ushort Size;
        public ushort Id;
        public uint Action;
        public uint Param;
        //public uint Type;

        public static MsgNoble Create(uint action, uint param)
        {
            var packet = new MsgNoble
            {
                Size = (ushort)sizeof(MsgNoble),
                Id = 2064,
                Action = action,
                Param = param
            };
            return packet;
        }
    }
}