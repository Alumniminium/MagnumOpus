using MagnumOpus.ECS;

namespace MagnumOpus.Simulation.Components
{
    [Component]
    public readonly struct ProfComponent
    {
        public readonly int EntityId;
        public readonly uint ChangedTick;
        public readonly ushort Id;
        public readonly ushort Level;
        public readonly ushort Experience;
        public readonly ushort ExperienceToNextLevel;
        
        public ProfComponent(int entityId, ushort skillId, ushort level, ushort experience, ushort experienceToNextLevel)
        {
            EntityId = entityId;
            ChangedTick = Game.CurrentTick;
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