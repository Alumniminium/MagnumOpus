using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Squiggly;
using MagnumOpus.Helpers;
using NttECS.ECS;

namespace MagnumOpus.Systems
{
    /// <summary>
    /// Handles experience point rewards, level progression, and automatic attribute allocation for leveling entities.
    /// Manages level-ups with health restoration, attribute point distribution based on profession tables.
    /// </summary>
    public sealed class ExpRewardSystem : NttSystem<LevelComponent, ExpRewardComponent>
    {
        /// <summary>
        /// Initializes the ExpRewardSystem with limited threading for experience processing.
        /// </summary>
        public ExpRewardSystem() : base("Exp Reward", threads: 1, log: false) { }

        /// <summary>
        /// Processes experience rewards, handles level-ups with automatic attribute allocation and health restoration.
        /// </summary>
        /// <param name="ntt">The entity receiving experience</param>
        /// <param name="levelComponent">Level component containing current level and experience data</param>
        /// <param name="experienceReward">Experience reward component specifying amount gained</param>
        public override void Update(in NTT ntt, ref LevelComponent levelComponent, ref ExpRewardComponent experienceReward)
        {
            levelComponent.Experience += (uint)experienceReward.Experience;

            if (levelComponent.Experience < levelComponent.ExperienceToNextLevel)
            {
                ntt.Remove<ExpRewardComponent>();
                if (IsLogging)
                    FConsole.WriteLine("{ntt} gained {exp} exp, now at {current}/{next} (level: {lvl})", ntt, experienceReward.Experience, levelComponent.Experience, levelComponent.ExperienceToNextLevel, levelComponent.Level);
                return;
            }

            ref var healthComponent = ref ntt.Get<HealthComponent>();
            levelComponent.Level++;
            var newLevel = levelComponent.Level;
            var profession = ntt.Get<ProfessionComponent>().Profession;
            levelComponent.Experience = 0;
            levelComponent.ExperienceToNextLevel = (uint)Collections.LevelExps.Values[levelComponent.Level - 1];
            healthComponent.Health = healthComponent.MaxHealth;

            var attributeAllocation = Collections.CqPointAllot.FirstOrDefault(x => x.level == newLevel && x.profession == (long)profession / 10);
            if (attributeAllocation != null)
            {
                ref var attributes = ref ntt.Get<AttributeComponent>();
                attributes.Agility = (ushort)attributeAllocation.Speed;
                attributes.Strength = (ushort)attributeAllocation.force;
                attributes.Vitality = (ushort)attributeAllocation.health;
                attributes.Spirit = (ushort)attributeAllocation.soul;
            }

            var levelUpMessage = MsgAction.Create(ntt.Id, 0, 0, 0, 0, Enums.MsgActionType.LevelUp);
            ntt.NetSync(ref levelUpMessage, true);

            if (IsLogging)
                FConsole.WriteLine("{ntt} gained {exp} exp and leveled to {lvl}, now at {current}/{next} (level: {lvl})", ntt, experienceReward.Experience, levelComponent.Level, levelComponent.Experience, levelComponent.ExperienceToNextLevel, levelComponent.Level);

            ntt.Remove<ExpRewardComponent>();
        }
    }
}