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

        public static MsgTeam Create(in PixelEntity human, MsgTeamAction action)
        {
            var msg = new MsgTeam
            {
                Size = (ushort)sizeof(MsgTeam),
                Id = 1023,
                Mode = action,
                TargetUniqueId = human.NetId
            };
            return msg;
        }

        public static MsgTeam CreateTeam(in PixelEntity human)
        {
            return Create(human, MsgTeamAction.Create);
        }

        public static MsgTeam DisbandTeam(in PixelEntity human)
        {
            return Create(human, MsgTeamAction.Dismiss);
        }

        public static MsgTeam Kick(in PixelEntity human)
        {
            return Create(human, MsgTeamAction.Kick);
        }

        public static MsgTeam Invite(in PixelEntity human)
        {
            return Create(human, MsgTeamAction.Invite);
        }

        public static MsgTeam Leave(in PixelEntity human)
        {
            return Create(human, MsgTeamAction.LeaveTeam);
        }
    }
}