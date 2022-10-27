using MagnumOpus.ECS;

namespace MagnumOpus.Simulation.Components
{
    [Component]
    public struct ExperienceComponent
    {
        public readonly int EntityId;
        public uint ChangedTick;

        public uint ExperienceToNextLevel;
        public uint Experience;

        public ExperienceComponent(int entityId, uint exp=0, uint expReq=0)
        {
            EntityId = entityId;
            Experience = exp;
            ExperienceToNextLevel = expReq;
            ChangedTick = PixelWorld.Tick;
        }
        public override int GetHashCode() => EntityId;
    }
}