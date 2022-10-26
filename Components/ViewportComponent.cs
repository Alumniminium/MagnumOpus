using System.Drawing;
using MagnumOpus.ECS;

namespace MagnumOpus.Simulation.Components
{
    [Component]
    public struct ViewportComponent
    {
        public readonly int EntityId;
        public uint ChangedTick;

        public readonly List<PixelEntity> EntitiesVisible;
        public readonly List<PixelEntity> EntitiesVisibleLast;
        public RectangleF Viewport;

        public ViewportComponent(int entityId, float viewDistance)
        {
            EntityId = entityId;
            Viewport = new RectangleF(0, 0, viewDistance, viewDistance);
            EntitiesVisible = new();
            EntitiesVisibleLast = new();
            ChangedTick = PixelWorld.Tick;
        }

        public override int GetHashCode() => EntityId;
    }
}