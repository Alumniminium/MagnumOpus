using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;
using NttECS.ECS;

namespace MagnumOpus.Systems
{
    public sealed class TeamSystem : NttSystem<TeamComponent>
    {
        public TeamSystem() : base("Team", threads: 1, log: false) { }

        // Manages team functionality including team creation, leader position updates, and experience
        // sharing. Handles team bonuses for Water Taoists (2x exp) and married couples (2x exp) with
        // level-based experience caps (level * 300). Creates teams on first tick, updates leader
        // position for navigation, and distributes shared experience among all team members.
        public override void Update(in NTT ntt, ref TeamComponent teamComponent)
        {
            // === HANDLE TEAM CREATION ===
            if (teamComponent.CreatedTick == NttWorld.Tick)
            {
                var teamCreateMessage = MsgTeam.CreateTeam(in ntt);
                var joinMessage = MsgTeamUpdate.JoinLeave(in ntt, MsgTeamMemberAction.AddMember);
                ntt.NetSync(ref teamCreateMessage);
                ntt.NetSync(ref joinMessage);

                // Set team leader status effect
                ref var statusEffects = ref ntt.Get<StatusEffectComponent>();
                statusEffects.Effects |= StatusEffect.TeamLeader;
            }

            // === UPDATE TEAM LEADER POSITION ===
            if (ntt == teamComponent.Leader)
            {
                ref readonly var position = ref ntt.Get<PositionComponent>();
                if (Tick % position.ChangedTick == 0)
                {
                    var leaderPositionMessage = MsgAction.Create(ntt.Id, ntt.Id, (ushort)position.Position.X, (ushort)position.Position.Y, 0, MsgActionType.QueryTeamLeaderPos);

                    // Send position update to all team members (member 0 is leader)
                    for (var memberIndex = 1; memberIndex < teamComponent.MemberCount + 1; memberIndex++)
                    {
                        var teamMember = teamComponent.Members[memberIndex];
                        if (teamMember.Id == ntt.Id)
                            continue;

                        teamMember.NetSync(ref leaderPositionMessage);
                    }

                    if (IsLogging)
                    {
                        var leaderMoveText = $"[{nameof(TeamSystem)}] {ntt.Id} moved to {position.Position}";
                        NetworkHelper.SendMsgTo(in ntt, leaderMoveText, MsgTextType.TopLeft);
                        FConsole.WriteLine(leaderMoveText);
                    }
                }
            }

            // === HANDLE EXPERIENCE SHARING ===
            if (ntt.Has<ExpRewardComponent>())
            {
                ref var experienceReward = ref ntt.Get<ExpRewardComponent>();
                ref var nameTag = ref ntt.Get<NameTagComponent>();
                ref var levelComponent = ref ntt.Get<LevelComponent>();

                var sharedExperience = experienceReward.Experience / teamComponent.MemberCount;

                // Apply TQ experience cap based on level
                if (sharedExperience > levelComponent.Level * 300)
                    sharedExperience = levelComponent.Level * 300;

                // Distribute experience to all team members
                for (var memberIndex = 0; memberIndex < teamComponent.MemberCount; memberIndex++)
                {
                    var teamMember = teamComponent.Members[memberIndex];

                    // Skip the member who gained the original experience
                    if (teamMember.Id == ntt.Id)
                        continue;

                    ref readonly var profession = ref teamMember.Get<ProfessionComponent>();
                    ref readonly var marriage = ref teamMember.Get<MarriageComponent>();
                    var memberExperience = sharedExperience;

                    // === APPLY EXPERIENCE BONUSES ===
                    // Water Taoist bonus (professions 133-135)
                    if ((int)profession.Profession is > 132 and < 136)
                        memberExperience *= 2;

                    // Marriage bonus (if married to the experience gainer)
                    if (marriage.SpouseId == ntt.Id)
                        memberExperience *= 2;

                    // Give experience to team member
                    var memberExperienceReward = new ExpRewardComponent(memberExperience);
                    teamMember.Set(ref memberExperienceReward);

                    // Notify team member of shared experience
                    var experienceShareMessage = $"{nameTag.Name} shared {memberExperience} experience with you!";
                    NetworkHelper.SendMsgTo(in teamMember, experienceShareMessage, MsgTextType.TopLeft);

                    if (IsLogging)
                        FConsole.WriteLine("[{system}] {member} -> {message}", nameof(TeamSystem), teamMember.Id, experienceShareMessage);
                }
            }
        }
    }
}