using System.Drawing;
using MagnumOpus.ECS;

namespace MagnumOpus.Simulation.Components
{
    [Component]
    public struct AABBComponent
    {
        public readonly int EntityId;
        public RectangleF AABB;
        public readonly List<PixelEntity> PotentialCollisions;

        public AABBComponent(int entityId, RectangleF aabb)
        {
            EntityId = entityId;
            AABB = aabb;
            PotentialCollisions = new();
        }
        public override int GetHashCode()
        {
            return EntityId;
        }
    }
}