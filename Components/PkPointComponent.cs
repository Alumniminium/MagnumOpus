using MagnumOpus.ECS;

namespace MagnumOpus.Simulation.Components
{
    [Component]
    public struct PkPointComponent
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

        public override int GetHashCode()
        {
            return EntityId;
        }
    }
}