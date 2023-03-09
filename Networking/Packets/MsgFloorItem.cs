using System.Runtime.InteropServices;
using MagnumOpus.Components;
using MagnumOpus.Components.Items;
using MagnumOpus.Components.Location;
using MagnumOpus.ECS;
using MagnumOpus.Enums;

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

        public static MsgFloorItem Create(in NTT item, MsgFloorItemType type)
        {
            ref readonly var pos = ref item.Get<PositionComponent>();
            ref readonly var itemComponent = ref item.Get<ItemComponent>();

            var packet = new MsgFloorItem
            {
                Size = (ushort)sizeof(MsgFloorItem),
                Id = (ushort)PacketId.MsgFloorItem,
                UniqueId = item.Id,
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
        public static void Process(NTT ntt, Memory<byte> memory)
        {
            var msg = Co2Packet.Deserialze<MsgFloorItem>(in memory);

            switch (msg.MsgFloorItemType)
            {
                case MsgFloorItemType.Pick:
                    {
                        ref readonly var item = ref NttWorld.GetEntity(msg.UniqueId);
                        if (item == default)
                            return;

                        var pic = new PickupRequestComponent( in item);
                        ntt.Set(ref pic);

                        ntt.NetSync(ref msg, true);
                        break;
                    }

                case MsgFloorItemType.None:
                    break;
                case MsgFloorItemType.Create:
                    break;
                case MsgFloorItemType.Delete:
                    break;
                case MsgFloorItemType.DisplayEffect:
                    break;
                case MsgFloorItemType.SynchroTrap:
                    break;
                case MsgFloorItemType.RemoveEffect:
                    break;
                default:
                    break;
            }
        }
    }
}