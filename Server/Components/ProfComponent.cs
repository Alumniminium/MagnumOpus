using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public partial struct ProfComponent
    {
        public long ChangedTick = NttWorld.Tick;
        private ushort _id;
        private ushort _level;
        private ushort _experience;
        private ushort _experienceToNextLevel;

        public ProfComponent() { }
        public ProfComponent(ushort skillId, ushort level, ushort experience, ushort experienceToNextLevel)
        {
            Id = skillId;                               // Uses generated property
            Level = level;                              // Uses generated property
            Experience = experience;                    // Uses generated property
            ExperienceToNextLevel = experienceToNextLevel; // Uses generated property
        }
    }
}