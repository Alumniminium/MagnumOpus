using System.Drawing;
using System.Runtime.InteropServices;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;

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

        public static MsgColor Create(in NTT ntt, Color color)
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
                Direction = bdy.Direction,
                Rgb = ColorToUInt(color),
                Type = 104
            };
            return packet;
        }
        public static MsgColor Create(in NTT ntt, uint color)
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
                Direction = bdy.Direction,
                Rgb = color,
                Type = 104
            };
            return packet;
        }

        private static uint ColorToUInt(Color c) => (uint)((c.A << 24) | (c.R << 16) | (c.G << 8) | (c.B << 0));
    }
}