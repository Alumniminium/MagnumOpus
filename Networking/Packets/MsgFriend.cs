using System.Buffers;
using System.Runtime.InteropServices;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Simulation.Components;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgFriend
    {
        public ushort Size;
        public ushort Id;
        public int UniqId;
        public MsgFriendActionType Action;
        public MsgFriendStatusType IsOnline;
        public long Unknow1;
        public short Unknow2;
        public fixed byte Name[16];

        public static byte[] Create(PixelEntity target, MsgFriendActionType action, MsgFriendStatusType status)
        {
            ref readonly var ntc = ref target.Get<NameTagComponent>();
            var packet = new MsgFriend
            {
                Size = (ushort)sizeof(MsgFriend),
                Id = 1019,
                Action = action,
                IsOnline = status,
                Unknow1 = 0,
                Unknow2 = 0,
                UniqId = target.Id,
            };
            for (byte i = 0; i < ntc.Name.Length; i++)
                packet.Name[i] = (byte)ntc.Name[i];
            return packet;
        }

        public static implicit operator byte[](MsgFriend msg)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(MsgUpdate));
            fixed (byte* p = buffer)
                *(MsgFriend*)p = *&msg;
            return buffer;
        }
    }
}