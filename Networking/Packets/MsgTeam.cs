using System.Buffers;
using System.Runtime.InteropServices;
using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgTeam
    {
        public ushort Size;
        public ushort Id;
        public MsgTeamAction Mode;
        public int TargetUniqueId;

        public static byte[] Create(PixelEntity human, MsgTeamAction action)
        {
            var msg = new MsgTeam
            {
                Size = (ushort)sizeof(MsgTeam),
                Id = 1023,
                Mode = action,
                TargetUniqueId = human.Id
            };
            return msg;
        }

        public static byte[] CreateTeam(PixelEntity human)
        {
            return Create(human, MsgTeamAction.Create);
        }

        public static byte[] DisbandTeam(PixelEntity human)
        {
            return Create(human, MsgTeamAction.Dismiss);
        }

        public static byte[] Kick(PixelEntity human)
        {
            return Create(human, MsgTeamAction.Kick);
        }

        public static byte[] Invite(PixelEntity human)
        {
            return Create(human, MsgTeamAction.Invite);
        }

        public static byte[] Leave(PixelEntity human)
        {
            return Create(human, MsgTeamAction.LeaveTeam);
        }

        public static unsafe implicit operator byte[](MsgTeam msg)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(MsgUpdate));
            fixed (byte* p = buffer)
                *(MsgTeam*)p = *&msg;
            return buffer;
        }
    }
}