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

        public static MsgTeam Create(in NTT ntt, MsgTeamAction action)
        {
            var msg = new MsgTeam
            {
                Size = (ushort)sizeof(MsgTeam),
                Id = 1023,
                Mode = action,
                TargetUniqueId = ntt.NetId
            };
            return msg;
        }

        public static MsgTeam CreateTeam(in NTT ntt) => Create(ntt, MsgTeamAction.Create);
        public static MsgTeam DisbandTeam(in NTT ntt) => Create(ntt, MsgTeamAction.Dismiss);
        public static MsgTeam Kick(in NTT ntt) => Create(ntt, MsgTeamAction.Kick);
        public static MsgTeam Invite(in NTT ntt) => Create(ntt, MsgTeamAction.Invite);
        public static MsgTeam Leave(in NTT ntt) => Create(ntt, MsgTeamAction.LeaveTeam);
    }
}