using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component][Save]
    public readonly struct PkPointComponent
    {
        public readonly int EntityId;
        public readonly byte Points;
        public readonly TimeSpan DecreaseTime;

        public PkPointComponent(int entityId, byte points, TimeSpan decreaseTime)
        {
            EntityId = entityId;
            Points = points;
            DecreaseTime = decreaseTime;
        }

        public override int GetHashCode() => EntityId;
    }
}