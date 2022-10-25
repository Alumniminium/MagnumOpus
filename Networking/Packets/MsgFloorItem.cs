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

        public static byte[] Create(PixelEntity item, MsgFloorItemType type)
        {
            ref readonly var phy = ref item.Get<BodyComponent>();
            ref readonly var trs = ref item.Get<TransformationComponent>();

            var look = trs.EntityId == item.Id ? trs.Look : phy.Look;

            var packet = new MsgFloorItem
            {
                Size = (ushort)sizeof(MsgFloorItem),
                Id = 1101,
                UniqueId = item.Id,
                X = (ushort)phy.Location.X,
                Y = (ushort)phy.Location.Y,
                ItemId = look,
                MsgFloorItemType = type,
            };
            return packet;
        }
        public static implicit operator byte[](MsgFloorItem msg)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(MsgUpdate));
            fixed (byte* p = buffer)
                *(MsgFloorItem*)p = *&msg;
            return buffer;
        }
    }
}