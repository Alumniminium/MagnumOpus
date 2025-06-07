using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;
using NttECS.ECS;

namespace MagnumOpus.Systems
{
    /// <summary>
    /// Manages team functionality including team creation, leader position updates, and experience sharing.
    /// Handles team bonuses for Water Taoists and married couples with level-based experience caps.
    /// </summary>
    public sealed class TeamSystem : NttSystem<TeamComponent>
    {
        /// <summary>
        /// Initializes the TeamSystem with limited threading for team processing.
        /// </summary>
        public TeamSystem() : base("Team", threads: 1) { }

        /// <summary>
        /// Processes team operations including creation, leader updates, and experience distribution with bonuses.
        /// </summary>
        /// <param name="ntt">The team member entity</param>
        /// <param name="teamComponent">Team component containing member and leadership data</param>
        public override void Update(in NTT ntt, ref TeamComponent teamComponent)
        {
            if (teamComponent.CreatedTick == NttWorld.Tick)
            {
                var teamCreateMessage = MsgTeam.CreateTeam(in ntt);
                var joinMessage = MsgTeamUpdate.JoinLeave(in ntt, MsgTeamMemberAction.AddMember);
                ntt.NetSync(ref teamCreateMessage);
                ntt.NetSync(ref joinMessage);
                ref var statusEffects = ref ntt.Get<StatusEffectComponent>();
                statusEffects.Effects |= StatusEffect.TeamLeader;
            }
            if (ntt.Id == teamComponent.Leader.Id)
            {
                ref readonly var position = ref ntt.Get<PositionComponent>();
                if (Tick % position.ChangedTick == 0)
                {
                    var leaderPositionMessage = MsgAction.Create(ntt.Id, ntt.Id, (ushort)position.Position.X, (ushort)position.Position.Y, 0, MsgActionType.QueryTeamLeaderPos);

                    // member 0 is the leader
                    for (var memberIndex = 1; memberIndex < teamComponent.MemberCount + 1; memberIndex++)
                    {
                        var teamMember = teamComponent.Members[memberIndex];
                        if (teamMember.Id == ntt.Id)
                            continue;

                        teamMember.NetSync(ref leaderPositionMessage);
                    }

                    var leaderMoveText = $"[{nameof(TeamSystem)}] {ntt.Id} moved to {position.Position}";
                    NetworkHelper.SendMsgTo(in ntt, leaderMoveText, MsgTextType.TopLeft);
                    FConsole.WriteLine(leaderMoveText);
                }
            }

            if (ntt.Has<ExpRewardComponent>())
            {
                ref var experienceReward = ref ntt.Get<ExpRewardComponent>();
                ref var nameTag = ref ntt.Get<NameTagComponent>();
                ref var levelComponent = ref ntt.Get<LevelComponent>();

                var sharedExperience = experienceReward.Experience / teamComponent.MemberCount;

                if (sharedExperience > levelComponent.Level * 300)  // TQ exp cap
                    sharedExperience = levelComponent.Level * 300; // 

                for (var memberIndex = 0; memberIndex < teamComponent.MemberCount; memberIndex++)
                {
                    var teamMember = teamComponent.Members[memberIndex];

                    if (teamMember.Id == ntt.Id)
                        continue;

                    ref readonly var profession = ref teamMember.Get<ProfessionComponent>();
                    ref readonly var marriage = ref teamMember.Get<MarriageComponent>();

                    if ((int)profession.Profession is > 132 and < 136) // Water Taoiust
                        sharedExperience *= 2;                    // Bonus Exp

                    if (marriage.SpouseId == ntt.Id) // Marriage bonus
                        sharedExperience *= 2;         // 

                    var memberExperienceReward = new ExpRewardComponent(sharedExperience);
                    teamMember.Set(ref memberExperienceReward);

                    var experienceShareMessage = $"{nameTag.Name} shared {sharedExperience} experience with you!";
                    NetworkHelper.SendMsgTo(in teamMember, experienceShareMessage, MsgTextType.TopLeft);
                    FConsole.WriteLine($"[{nameof(TeamSystem)}] {teamMember.Id} -> {experienceShareMessage}");
                }
            }
        }
    }
}