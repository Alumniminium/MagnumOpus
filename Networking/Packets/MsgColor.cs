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

        public static byte[] Create(PixelEntity target, Color color)
        {
            ref readonly var phy = ref target.Get<BodyComponent>();
            var packet = new MsgColor
            {
                Size = (ushort)sizeof(MsgColor),
                Id = 1010,
                Timestamp = Environment.TickCount,
                UniqueId = target.Id,
                X = (ushort)phy.Location.X,
                Y = (ushort)phy.Location.Y,
                Direction = (ushort)phy.Direction,
                Rgb = ColorToUInt(color),
                Type = 104
            };
            return packet;
        }
        public static byte[] Create(PixelEntity obj, uint color)
        {
            ref readonly var phy = ref obj.Get<BodyComponent>();
            var packet = new MsgColor
            {
                Size = (ushort)sizeof(MsgColor),
                Id = 1010,
                Timestamp = Environment.TickCount,
                UniqueId = obj.Id,
                X = (ushort)phy.Location.X,
                Y = (ushort)phy.Location.Y,
                Direction = (ushort)phy.Direction,
                Rgb = color,
                Type = 104
            };
            return packet;
        }

        private static uint ColorToUInt(Color color)
        {
            return (uint)((color.A << 24) | (color.R << 16) | (color.G << 8) | (color.B << 0));
        }

        public static unsafe implicit operator byte[](MsgColor msg)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(MsgUpdate));
            fixed (byte* p = buffer)
                *(MsgColor*)p = *&msg;
            return buffer;
        }
    }
}