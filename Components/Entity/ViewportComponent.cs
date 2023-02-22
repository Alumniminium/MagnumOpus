using System.Drawing;
using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct ViewportComponent
    {
        public readonly int EntityId;
        public long ChangedTick;

        public readonly HashSet<NTT> EntitiesVisible = new();
        public readonly HashSet<NTT> EntitiesVisibleLast=new ();
        public RectangleF Viewport;

        public ViewportComponent(int entityId, float viewDistance)
        {
            EntityId = entityId;
            Viewport = new RectangleF(0, 0, viewDistance, viewDistance);
            ChangedTick = NttWorld.Tick;
        }

        public override int GetHashCode() => EntityId;
    }
}