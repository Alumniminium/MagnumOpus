using System.Buffers;
using System.Drawing;
using System.Runtime.InteropServices;
using MagnumOpus.ECS;
using MagnumOpus.Simulation.Components;

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
        public ushort Direction;
        public uint Type;

        public static Memory<byte> Create(in PixelEntity ntt, Color color)
        {
            ref readonly var bdy = ref ntt.Get<BodyComponent>();
            ref readonly var pos = ref ntt.Get<PositionComponent>();

            var packet = new MsgColor
            {
                Size = (ushort)sizeof(MsgColor),
                Id = 1010,
                Timestamp = Environment.TickCount,
                UniqueId = ntt.Id,
                X = (ushort)pos.Position.X,
                Y = (ushort)pos.Position.Y,
                Direction = (ushort)bdy.Direction,
                Rgb = ColorToUInt(color),
                Type = 104
            };
            return packet;
        }
        public static Memory<byte> Create(in PixelEntity obj, uint color)
        {
            ref readonly var bdy = ref obj.Get<BodyComponent>();
            ref readonly var pos = ref obj.Get<PositionComponent>();

            var packet = new MsgColor
            {
                Size = (ushort)sizeof(MsgColor),
                Id = 1010,
                Timestamp = Environment.TickCount,
                UniqueId = obj.Id,
                X = (ushort)pos.Position.X,
                Y = (ushort)pos.Position.Y,
                Direction = (ushort)bdy.Direction,
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