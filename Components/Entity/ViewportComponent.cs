using System.Drawing;
using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct ViewportComponent
    {
        public readonly int EntityId;
        public long ChangedTick;

        public readonly HashSet<NTT> EntitiesVisible;
        public readonly HashSet<NTT> EntitiesVisibleLast;
        public RectangleF Viewport;

        public ViewportComponent(int entityId, float viewDistance)
        {
            EntityId = entityId;
            Viewport = new RectangleF(0, 0, viewDistance, viewDistance);
            EntitiesVisible = new();
            EntitiesVisibleLast = new();
            ChangedTick = NttWorld.Tick;
        }

        public override int GetHashCode() => EntityId;
    }
}