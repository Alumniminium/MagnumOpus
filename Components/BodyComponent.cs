using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Components
{
    [Component]
    public struct BodyComponent
    {
        public readonly int EntityId;
        public long ChangedTick;
        public Direction Direction;
        public uint Look;

        public BodyComponent(int entityId, uint look = 1003, Direction direction = Direction.South)
        {
            EntityId = entityId;
            ChangedTick = NttWorld.Tick;
            Look = look;
            Direction = direction;
        }

        public override int GetHashCode() => EntityId;
    }
}
