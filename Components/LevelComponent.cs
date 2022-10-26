using MagnumOpus.ECS;

namespace MagnumOpus.Simulation.Components
{

    [Component]
    public struct LevelComponent
    {
        public readonly int EntityId;
        public byte Level;
        public uint ExperienceToNextLevel;
        public uint Experience;
        public uint ChangedTick;

        public LevelComponent(int entityId, byte level=1, uint exp=0, uint expReq=0)
        {
            EntityId = entityId;
            Level = level;
            Experience = exp;
            ExperienceToNextLevel = expReq;
            ChangedTick = PixelWorld.Tick;
        }
        public override int GetHashCode()
        {
            return EntityId;
        }
    }
}