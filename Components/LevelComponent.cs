using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct LevelComponent
    {
        public readonly int EntityId;
        public long ChangedTick;
        public byte Level;
        public uint Experience;
        public uint ExperienceToNextLevel;

        public LevelComponent(int entityId, byte level=1, uint experience=0, uint experienceToNextLevel=120)
        {
            EntityId = entityId;
            Level = level;
            Experience = experience;
            ExperienceToNextLevel = experienceToNextLevel;
            ChangedTick = PixelWorld.Tick;
        }
        public override int GetHashCode() => EntityId;
    }
}