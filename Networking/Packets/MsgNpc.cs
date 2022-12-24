using System.Buffers;
using System.Runtime.InteropServices;
using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgNpc
    {
        public ushort Size;
        public ushort Id;
        public int UniqId;//4
        public int Param;//8
        public MsgNpcAction Action;//12
        public short Type;//14

        public static MsgNpc Create(in PixelEntity target, ushort param, MsgNpcAction action)
        {
            var packet = new MsgNpc
            {
                Size = (ushort)sizeof(MsgNpc),
                Id = 2031,
                UniqId = target.NetId,
                Param = param,
                Action = action,
                Type = 26
            };
            return packet;
        }
    }
}