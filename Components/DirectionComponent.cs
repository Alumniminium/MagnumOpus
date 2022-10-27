using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Components
{
    [Component]
    public struct DirectionComponent
    {
        public readonly int EntityId;
        public uint ChangedTick;
        public Direction Direction;

        public DirectionComponent(int entityId, Direction direction = Direction.South)
        {
            EntityId = entityId;
            Direction = direction;
            ChangedTick = PixelWorld.Tick;
        }

        public override int GetHashCode() => EntityId;
    }
}