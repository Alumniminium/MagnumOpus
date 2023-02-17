using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class TeamSystem : NttSystem<TeamComponent>
    {
        public TeamSystem() : base("Team", threads: 2) { }

        public override void Update(in NTT ntt, ref TeamComponent team)
        {
            if(team.CreatedTick == NttWorld.Tick)
            {
                var msg = MsgTeam.CreateTeam(in ntt);
                var msgJoin = MsgTeamUpdate.JoinLeave(in ntt, MsgTeamMemberAction.AddMember);
                ntt.NetSync(ref msg);
                ntt.NetSync(ref msgJoin);
                ref var eff = ref ntt.Get<StatusEffectComponent>();
                eff.Effects |= StatusEffect.TeamLeader;

                var msgUpdate = MsgUserAttrib.Create(ntt.NetId, (ulong)eff.Effects,MsgUserAttribType.StatusEffect);
                ntt.NetSync(ref msgUpdate, true);
            }
            if (ntt.Id == team.Leader.Id)
            {
                ref readonly var pos = ref ntt.Get<PositionComponent>();
                if(pos.ChangedTick == NttWorld.Tick)
                {
                    var leaderPos = MsgAction.Create(ntt.NetId, ntt.NetId, (ushort)pos.Position.X, (ushort)pos.Position.Y, 0, MsgActionType.QueryTeamLeaderPos);

                    // member 0 is the leader
                    for(int i = 1; i < team.MemberCount+1; i++)
                    {
                        var member = team.Members[i];
                        if (member.Id == ntt.Id)
                            continue;

                        member.NetSync(ref leaderPos);
                    }

                    var leaderMoveMsg = $"[{nameof(TeamSystem)}] {ntt.NetId} moved to {pos.Position}";
                    NetworkHelper.SendMsgTo(in ntt, leaderMoveMsg, MsgTextType.TopLeft);
                    FConsole.WriteLine(leaderMoveMsg);
                }
            }

            if (ntt.Has<ExpRewardComponent>())
            {
                ref var rew = ref ntt.Get<ExpRewardComponent>();
                ref var ntc = ref ntt.Get<NameTagComponent>();
                ref var lvl = ref ntt.Get<LevelComponent>();

                var sharedExp = rew.Experience / team.MemberCount;

                if(sharedExp > lvl.Level * 300)  // TQ exp cap
                    sharedExp = lvl.Level * 300; // 

                for (int i = 0; i < team.MemberCount; i++)
                {
                    var member = team.Members[i];

                    if (member.Id == ntt.Id)
                        continue;

                    ref readonly var pro = ref member.Get<ClassComponent>();
                    ref readonly var mrg = ref member.Get<MarriageComponent>();
                        
                    if ((int)pro.Class is > 132 and < 136) // Water Taoiust
                        sharedExp *= 2;                    // Bonus Exp

                    if(mrg.EntityId == member.Id)   // 
                        if (mrg.SpouseId == ntt.Id) // Marriage bonus
                            sharedExp *= 2;         // 

                    var sexp = new ExpRewardComponent(in member, sharedExp);
                    member.Set(ref sexp);

                    var expShareMsg = $"{ntc.Name} shared {sharedExp} experience with you!";
                    NetworkHelper.SendMsgTo(in member, expShareMsg, MsgTextType.TopLeft);
                    FConsole.WriteLine($"[{nameof(TeamSystem)}] {member.Id} -> {expShareMsg}");
                }
            }
        }
    }
}