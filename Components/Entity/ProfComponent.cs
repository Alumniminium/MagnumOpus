using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component][Save]
    public readonly struct ProfComponent
    {
        public readonly int EntityId;
        public readonly long ChangedTick;
        public readonly ushort Id;
        public readonly ushort Level;
        public readonly ushort Experience;
        public readonly ushort ExperienceToNextLevel;
        
        public ProfComponent(int entityId, ushort skillId, ushort level, ushort experience, ushort experienceToNextLevel)
        {
            EntityId = entityId;
            ChangedTick = NttWorld.Tick;
            Id = skillId;
            Level = level;
            Experience = experience;
            ExperienceToNextLevel = experienceToNextLevel;
        }
        public override int GetHashCode()
        {
            return EntityId;
        }
    }
}