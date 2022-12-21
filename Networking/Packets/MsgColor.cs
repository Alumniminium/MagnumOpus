using System.Buffers;
using System.Drawing;
using System.Runtime.InteropServices;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Components;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgColor
    {
        public ushort Size;
        public ushort Id;
        public int Timestamp;
        public int UniqueId;
        public uint Rgb;
        public ushort X;
        public ushort Y;
        public Direction Direction;
        public uint Type;

        public static Memory<byte> Create(in PixelEntity ntt, Color color)
        {
            ref readonly var bdy = ref ntt.Get<BodyComponent>();
            ref readonly var pos = ref ntt.Get<PositionComponent>();
            ref readonly var dir = ref ntt.Get<DirectionComponent>();

            var packet = new MsgColor
            {
                Size = (ushort)sizeof(MsgColor),
                Id = 1010,
                Timestamp = Environment.TickCount,
                UniqueId = ntt.NetId,
                X = (ushort)pos.Position.X,
                Y = (ushort)pos.Position.Y,
                Direction = dir.Direction,
                Rgb = ColorToUInt(color),
                Type = 104
            };
            return packet;
        }
        public static Memory<byte> Create(in PixelEntity ntt, uint color)
        {
            ref readonly var bdy = ref ntt.Get<BodyComponent>();
            ref readonly var pos = ref ntt.Get<PositionComponent>();
            ref readonly var dir = ref ntt.Get<DirectionComponent>();

            var packet = new MsgColor
            {
                Size = (ushort)sizeof(MsgColor),
                Id = 1010,
                Timestamp = Environment.TickCount,
                UniqueId = ntt.NetId,
                X = (ushort)pos.Position.X,
                Y = (ushort)pos.Position.Y,
                Direction = dir.Direction,
                Rgb = color,
                Type = 104
            };
            return packet;
        }

        private static uint ColorToUInt(Color color)
        {
            return (uint)((color.A << 24) | (color.R << 16) | (color.G << 8) | (color.B << 0));
        }

        public static unsafe implicit operator Memory<byte>(MsgColor msg)
        {
            var buffer = new byte[sizeof(MsgColor)];
            fixed (byte* p = buffer)
                *(MsgColor*)p = *&msg;
            return buffer;
        }
    }
}