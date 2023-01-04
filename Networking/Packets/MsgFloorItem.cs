using System.Buffers;
using System.Runtime.InteropServices;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgFloorItem
    {
        public ushort Size;
        public ushort Id;
        public uint UniqueId;
        public uint ItemId;
        public ushort X, Y;
        public MsgFloorItemType MsgFloorItemType;

        public static MsgFloorItem Create(in PixelEntity item, MsgFloorItemType type)
        {
            ref readonly var bdy = ref item.Get<BodyComponent>();
            ref readonly var trs = ref item.Get<TransformationComponent>();
            ref readonly var pos = ref item.Get<PositionComponent>();

            var look = trs.EntityId == item.NetId ? trs.Look : bdy.Look;

            var packet = new MsgFloorItem
            {
                Size = (ushort)sizeof(MsgFloorItem),
                Id = 1101,
                UniqueId = (uint)item.NetId,
                X = (ushort)pos.Position.X,
                Y = (ushort)pos.Position.Y,
                ItemId = look,
                MsgFloorItemType = type,
            };
            return packet;
        }
        public static MsgFloorItem Create(uint uid, ushort x, ushort y, uint look, MsgFloorItemType type)
        {
            var packet = new MsgFloorItem
            {
                Size = (ushort)sizeof(MsgFloorItem),
                Id = 1101,
                UniqueId = uid,
                X = x,
                Y = y,
                ItemId = look,
                MsgFloorItemType = type,
            };
            return packet;
        }
    }
}