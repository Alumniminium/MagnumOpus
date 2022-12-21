using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Components
{
    [Component]
    public readonly struct WalkComponent
    {
        public readonly int EntityId;
        public readonly uint CreatedTick;
        public readonly uint ChangedTick;
        public readonly Direction Direction;
        public readonly byte RawDirection;
        public readonly bool IsRunning;

        public WalkComponent(int entityId, byte direction, bool isRunning)
        {
            EntityId = entityId;
            CreatedTick = PixelWorld.Tick;
            ChangedTick = PixelWorld.Tick;
            RawDirection = direction;
            Direction = (Direction)(direction%8);
            IsRunning = isRunning;
        }

        public override int GetHashCode() => EntityId;
    }
}