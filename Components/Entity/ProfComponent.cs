using MagnumOpus.ECS;
namespace MagnumOpus.Components.Entity
{
    [Component]
    [Save]
    public readonly struct ProfComponent
    {
        public readonly long ChangedTick;
        public readonly ushort Id;
        public readonly ushort Level;
        public readonly ushort Experience;
        public readonly ushort ExperienceToNextLevel;

        public ProfComponent(ushort skillId, ushort level, ushort experience, ushort experienceToNextLevel)
        {
            ChangedTick = NttWorld.Tick;
            Id = skillId;
            Level = level;
            Experience = experience;
            ExperienceToNextLevel = experienceToNextLevel;
        }
    }
}