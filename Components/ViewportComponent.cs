using System.Drawing;
using MagnumOpus.ECS;

namespace MagnumOpus.Simulation.Components
{
    [Component]
    public struct ViewportComponent
    {
        public readonly int EntityId;
        public readonly List<PixelEntity> EntitiesVisible;
        public readonly List<PixelEntity> EntitiesVisibleLast;
        public RectangleF Viewport;

        public ViewportComponent(int entityId, float viewDistance)
        {
            EntityId = entityId;
            Viewport = new RectangleF(0, 0, viewDistance, viewDistance);
            EntitiesVisible = new();
            EntitiesVisibleLast = new();
        }
        public override int GetHashCode()
        {
            return EntityId;
        }
    }
}