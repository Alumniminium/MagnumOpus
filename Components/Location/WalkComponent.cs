using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Components.Location
{
    [Component]
    public readonly struct WalkComponent
    {
        public readonly int EntityId;
        public readonly long ChangedTick;
        public readonly Direction Direction;
        public readonly bool IsRunning;

        public WalkComponent(int entityId, byte direction, bool isRunning)
        {
            EntityId = entityId;
            ChangedTick = NttWorld.Tick;
            Direction = (Direction)(direction % 8);
            IsRunning = isRunning;
        }

        public override int GetHashCode() => EntityId;
    }
}