using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Squiggly;
using MagnumOpus.Helpers;
using NttECS.ECS;

namespace MagnumOpus.Systems;

public sealed class LevelingSystem : NttSystem<LevelComponent, ExpRewardComponent>
{
    public LevelingSystem() : base("Leveling", threads: 1, log: false) { }

    // Handles experience rewards and level progression. When an entity gains enough experience
    // to level up, this system automatically increases their level, restores their health to
    // maximum, and applies profession-based attribute allocations. A level-up animation is
    // broadcast to nearby players, and the ExpRewardComponent is removed after processing.
    public override void Update(in NTT ntt, ref LevelComponent lvlC, ref ExpRewardComponent experienceReward)
    {
        // Grant experience to the entity
        var expGained = (uint)experienceReward.Experience;
        lvlC.Experience += expGained;

        // Check if entity has enough experience to level up
        var canLevelUp = lvlC.Experience >= lvlC.ExperienceToNextLevel;

        if (!canLevelUp)
        {
            // Not enough experience for level up - just log the gain and cleanup
            if (IsLogging)
                FConsole.WriteLine("{ntt} gained {exp} exp, now at {current}/{next} (level: {lvl})", ntt, expGained, lvlC.Experience, lvlC.ExperienceToNextLevel, lvlC.Level);

            ntt.Remove<ExpRewardComponent>();
            return;
        }

        // === LEVEL UP PROCESSING ===

        // Advance to next level
        lvlC.Level++;
        var newLvl = lvlC.Level;

        // Reset experience and calculate next level requirement
        lvlC.Experience = 0;
        lvlC.ExperienceToNextLevel = (uint)Collections.LevelExps.Values[newLvl - 1];

        // Restore health to maximum on level up
        ref var healthComponent = ref ntt.Get<HealthComponent>();
        healthComponent.Health = healthComponent.MaxHealth;

        // Apply automatic attribute allocation based on profession
        var profession = ntt.Get<ProfessionComponent>().Profession;
        var professionBase = (long)profession / 10;

        var attributeAllocation = Collections.CqPointAllot.FirstOrDefault(x => x.level == newLvl && x.profession == professionBase);

        if (attributeAllocation is not null)
        {
            ref var attributes = ref ntt.Get<AttributeComponent>();
            attributes.Strength = (ushort)attributeAllocation.force;
            attributes.Agility = (ushort)attributeAllocation.Speed;
            attributes.Vitality = (ushort)attributeAllocation.health;
            attributes.Spirit = (ushort)attributeAllocation.soul;
        }

        // Broadcast level up animation to nearby players
        var levelUpMessage = MsgAction.Create(ntt.Id, 0, 0, 0, 0, Enums.MsgActionType.LevelUp);
        ntt.NetSync(ref levelUpMessage, true);

        // Log the level up event
        if (IsLogging)
            FConsole.WriteLine("{ntt} gained {exp} exp and leveled to {lvl}, now at {current}/{next}", ntt, expGained, newLvl, lvlC.Experience, lvlC.ExperienceToNextLevel);

        // Clean up the reward component
        ntt.Remove<ExpRewardComponent>();
    }
}