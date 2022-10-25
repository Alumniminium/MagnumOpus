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

        public static byte[] Create(PixelEntity target, ushort param, MsgNpcAction action)
        {
            var packet = new MsgNpc
            {
                Size = (ushort)sizeof(MsgNpc),
                Id = 2031,
                UniqId = target.Id,
                Param = param,
                Action = action,
                Type = 26
            };
            return packet;
        }

        public static unsafe implicit operator byte[](MsgNpc msg)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(MsgUpdate));
            fixed (byte* p = buffer)
                *(MsgNpc*)p = *&msg;
            return buffer;
        }
    }
}