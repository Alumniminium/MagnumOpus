using System.Buffers;
using System.Runtime.InteropServices;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Simulation.Components;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgFloorItem
    {
        public ushort Size;
        public ushort Id;
        public int UniqueId;
        public uint ItemId;
        public ushort X, Y;
        public MsgFloorItemType MsgFloorItemType;

        public static Memory<byte> Create(in PixelEntity item, MsgFloorItemType type)
        {
            ref readonly var bdy = ref item.Get<BodyComponent>();
            ref readonly var trs = ref item.Get<TransformationComponent>();
            ref readonly var pos = ref item.Get<PositionComponent>();

            var look = trs.EntityId == item.Id ? trs.Look : bdy.Look;

            var packet = new MsgFloorItem
            {
                Size = (ushort)sizeof(MsgFloorItem),
                Id = 1101,
                UniqueId = item.Id,
                X = (ushort)pos.Position.X,
                Y = (ushort)pos.Position.Y,
                ItemId = look,
                MsgFloorItemType = type,
            };
            return packet;
        }
        public static implicit operator Memory<byte>(MsgFloorItem msg)
        {
            var buffer = new byte[sizeof(MsgFloorItem)];
            fixed (byte* p = buffer)
                *(MsgFloorItem*)p = *&msg;
            return buffer;
        }
    }
}