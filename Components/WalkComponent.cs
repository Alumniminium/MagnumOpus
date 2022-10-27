using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Simulation.Components
{
    [Component]
    public readonly struct WalkComponent
    {
        public readonly int EntityId;
        public readonly uint CreatedTick;
        public readonly uint ChangedTick;
        public readonly Direction Direction;
        public readonly bool IsRunning;

        public WalkComponent(int entityId, Direction direction, bool isRunning)
        {
            EntityId = entityId;
            CreatedTick = PixelWorld.Tick;
            ChangedTick = PixelWorld.Tick;
            Direction = direction;
            IsRunning = isRunning;
        }

        public override int GetHashCode() => EntityId;
    }
}