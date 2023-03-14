using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component]
    [Save]
    public struct ProfComponent
    {
        public long ChangedTick;
        public ushort Id;
        public ushort Level;
        public ushort Experience;
        public ushort ExperienceToNextLevel;

        public ProfComponent() => ChangedTick = NttWorld.Tick;
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