using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component(SaveEnabled: true)]
    public struct ProfComponent(ushort skillId, ushort level, ushort experience, ushort experienceToNextLevel)
    {
        public long ChangedTick = NttWorld.Tick;
        public ushort Id = skillId;
        public ushort Level = level;
        public ushort Experience = experience;
        public ushort ExperienceToNextLevel = experienceToNextLevel;
    }
}