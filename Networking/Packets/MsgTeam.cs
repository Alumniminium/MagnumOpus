using System.Runtime.InteropServices;
using MagnumOpus.Components;
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

        [PacketHandler(PacketId.MsgTeam)]
        public static void Process(NTT ntt, Memory<byte> packet)
        {
            var msg = Co2Packet.Deserialze<MsgTeam>(in packet);
            switch (msg.Mode)
            {
                case MsgTeamAction.Create:
                    if(ntt.Has<TeamComponent>()) 
                        return;
                    var tc = new TeamComponent(in ntt);
                    ntt.Set(ref tc);
                    break;
                case MsgTeamAction.Dismiss:
                    ntt.Remove<TeamComponent>();
                    break;
                case MsgTeamAction.Kick:
                    break;
                case MsgTeamAction.Invite:
                    var target = NttWorld.GetEntityByNetId(msg.TargetUniqueId);
                    if (target != default)
                    {
                        var pkt = Invite(in ntt);
                        target.NetSync(ref pkt);
                    }
                    break;
                case MsgTeamAction.AcceptInvite:
                    var leader = NttWorld.GetEntityByNetId(msg.TargetUniqueId);
                    if (leader != default)
                    {
                        ref var team = ref leader.Get<TeamComponent>();
                        if (team.MemberCount < team.Members.Length)
                        {
                            team.Members[team.MemberCount] = ntt;
                            team.MemberCount++;

                            for(int i = 0; i < team.MemberCount; i++)
                            {
                                var member = team.Members[i];
                                var memberMsg = MsgTeamUpdate.JoinLeave(in member, MsgTeamMemberAction.AddMember);
                                ntt.NetSync(ref memberMsg);
                                leader.NetSync(ref memberMsg);
                            }
                        }
                    }
                    break;
                case MsgTeamAction.LeaveTeam:
                    break;
            }
        }
    }
}