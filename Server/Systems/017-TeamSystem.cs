using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;
using NttECS.ECS;

namespace MagnumOpus.Systems;

public sealed class TeamSystem : NttSystem<TeamComponent, PositionComponent>
{
    public TeamSystem() : base("Team", threads: 1, log: false) { }

    // Manages team functionality including team creation, leader position updates, and experience
    // sharing. Handles team bonuses for Water Taoists (2x exp) and married couples (2x exp) with
    // level-based experience caps (level * 300). Creates teams on first tick, updates leader
    // position for navigation, and distributes shared experience among all team members.
    public override void Update(in NTT ntt, ref TeamComponent teamC, ref PositionComponent posC)
    {
        // === HANDLE TEAM CREATION ===
        if (teamC.CreatedTick == NttWorld.Tick)
        {
            var teamCreateMsg = MsgTeam.CreateTeam(in ntt);
            var joinMsg = MsgTeamUpdate.JoinLeave(in ntt, MsgTeamMemberAction.AddMember);
            ntt.NetSync(ref teamCreateMsg);
            ntt.NetSync(ref joinMsg);

            // Set team leader status effect
            ref var statusEffects = ref ntt.Get<StatusEffectComponent>();
            statusEffects.Effects |= StatusEffect.TeamLeader;
        }

        // === UPDATE TEAM LEADER POSITION ===
        if (ntt == teamC.Leader)
        {
            if (teamC.LastLeaderPosTick != posC.ChangedTick)
            {
                teamC.LastLeaderPosTick = posC.ChangedTick;
                var leaderPosMsg = MsgAction.Create(ntt.Id, ntt.Id, (ushort)posC.Position.X, (ushort)posC.Position.Y, 0, MsgActionType.QueryTeamLeaderPos);

                // Send position update to all team members (member 0 is leader)
                for (var memberIdx = 1; memberIdx < teamC.MemberCount + 1; memberIdx++)
                {
                    var teamMember = teamC.Members[memberIdx];
                    if (teamMember.Id == ntt.Id)
                        continue;

                    teamMember.NetSync(ref leaderPosMsg);
                }

                if (IsLogging)
                {
                    var leaderMoveText = $"[{nameof(TeamSystem)}] {ntt.Id} moved to {posC.Position}";
                    NetworkHelper.SendMsgTo(in ntt, leaderMoveText, MsgTextType.TopLeft);
                    FConsole.WriteLine(leaderMoveText);
                }
            }
        }

        // === HANDLE EXPERIENCE SHARING ===
        if (ntt.Has<ExpRewardComponent>())
        {
            ref var expRewardC = ref ntt.Get<ExpRewardComponent>();
            ref var nameC = ref ntt.Get<NameTagComponent>();
            ref var lvlC = ref ntt.Get<LevelComponent>();

            var sharedExp = expRewardC.Experience / teamC.MemberCount;

            // Apply TQ experience cap based on level
            if (sharedExp > lvlC.Level * 300)
                sharedExp = lvlC.Level * 300;

            // Distribute experience to all team members
            for (var memberIndex = 0; memberIndex < teamC.MemberCount; memberIndex++)
            {
                var memberNtt = teamC.Members[memberIndex];

                // Skip the member who gained the original experience
                if (memberNtt.Id == ntt.Id)
                    continue;

                ref readonly var job = ref memberNtt.Get<ProfessionComponent>();
                ref readonly var marriage = ref memberNtt.Get<MarriageComponent>();
                var memberExp = sharedExp;

                // === APPLY EXPERIENCE BONUSES ===
                // Water Taoist bonus (professions 133-135)
                if ((int)job.Profession is > 132 and < 136)
                    memberExp *= 2;

                // Marriage bonus (if married to the experience gainer)
                if (marriage.SpouseId == ntt.Id)
                    memberExp *= 2;

                // Give experience to team member
                var memberExpReward = new ExpRewardComponent(memberExp);
                memberNtt.Set(ref memberExpReward);

                // Notify team member of shared experience
                var expShareMsg = $"{nameC.Name} shared {memberExp} experience with you!";
                NetworkHelper.SendMsgTo(in memberNtt, expShareMsg, MsgTextType.TopLeft);

                if (IsLogging)
                    FConsole.WriteLine("[{system}] {member} -> {message}", nameof(TeamSystem), memberNtt.Id, expShareMsg);
            }
        }
    }
}