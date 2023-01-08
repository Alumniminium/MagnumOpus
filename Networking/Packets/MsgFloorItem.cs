using System.Buffers;
using System.Runtime.InteropServices;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using Microsoft.VisualStudio.TextTemplating;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgFloorItem
    {
        public ushort Size;
        public ushort Id;
        public int UniqueId;
        public int ItemId;
        public ushort X, Y;
        public MsgFloorItemType MsgFloorItemType;

        public static MsgFloorItem Create(in PixelEntity item, MsgFloorItemType type)
        {
            ref readonly var pos = ref item.Get<PositionComponent>();
            ref readonly var itemComponent = ref item.Get<ItemComponent>();

            var packet = new MsgFloorItem
            {
                Size = (ushort)sizeof(MsgFloorItem),
                Id = (ushort)PacketId.MsgFloorItem,
                UniqueId = item.NetId,
                X = (ushort)pos.Position.X,
                Y = (ushort)pos.Position.Y,
                ItemId = itemComponent.Id,
                MsgFloorItemType = type,
            };
            return packet;
        }
        public static MsgFloorItem Create(int uid, ushort x, ushort y, int look, MsgFloorItemType type)
        {
            var packet = new MsgFloorItem
            {
                Size = (ushort)sizeof(MsgFloorItem),
                Id = (ushort)PacketId.MsgFloorItem,
                UniqueId = uid,
                X = x,
                Y = y,
                ItemId = look,
                MsgFloorItemType = type,
            };
            return packet;
        }

        [PacketHandler(PacketId.MsgFloorItem)]
        public static void Process(PixelEntity ntt, Memory<byte> memory)
        {
            var msg = Co2Packet.Deserialze<MsgFloorItem>(in memory);

            switch (msg.MsgFloorItemType)
            {
                case MsgFloorItemType.Pick:
                {
                    ref readonly var item = ref PixelWorld.GetEntityByNetId(msg.UniqueId);
                    var pic = new PickupRequestComponent(ntt.Id, in item);
                    ntt.Add(ref pic);
                    ntt.NetSync(memory[0..msg.Size], true);
                    break;
                }
            }
        }
    }
}