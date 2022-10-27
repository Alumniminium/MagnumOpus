using MagnumOpus.ECS;

namespace MagnumOpus.Simulation.Components
{

    [Component]
    public readonly struct LevelComponent
    {
        public readonly int EntityId;
        public readonly uint ChangedTick;
        public readonly byte Level;

        public LevelComponent(int entityId, byte level=1)
        {
            EntityId = entityId;
            Level = level;
            ChangedTick = PixelWorld.Tick;
        }
        public override int GetHashCode() => EntityId;
    }
}