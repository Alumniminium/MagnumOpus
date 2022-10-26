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

        public static Memory<byte> Create(in PixelEntity human, MsgTeamAction action)
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

        public static Memory<byte> CreateTeam(in PixelEntity human)
        {
            return Create(human, MsgTeamAction.Create);
        }

        public static Memory<byte> DisbandTeam(in PixelEntity human)
        {
            return Create(human, MsgTeamAction.Dismiss);
        }

        public static Memory<byte> Kick(in PixelEntity human)
        {
            return Create(human, MsgTeamAction.Kick);
        }

        public static Memory<byte> Invite(in PixelEntity human)
        {
            return Create(human, MsgTeamAction.Invite);
        }

        public static Memory<byte> Leave(in PixelEntity human)
        {
            return Create(human, MsgTeamAction.LeaveTeam);
        }

        public static unsafe implicit operator Memory<byte>(MsgTeam msg)
        {
            var buffer = new byte[sizeof(MsgTeam)];
            fixed (byte* p = buffer)
                *(MsgTeam*)p = *&msg;
            return buffer;
        }
    }
}