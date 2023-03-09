using System.Collections.Concurrent;
using System.Drawing;
using MagnumOpus.ECS;

namespace MagnumOpus.Components.Entity
{
    [Component]
    public struct ViewportComponent
    {
        public readonly int EntityId;
        public long ChangedTick;

        public readonly ConcurrentDictionary<int, NTT> EntitiesVisible;
        public readonly ConcurrentDictionary<int, NTT> EntitiesVisibleLast;
        public RectangleF Viewport;
        internal bool Dirty;

        public ViewportComponent(int entityId, float viewDistance)
        {
            EntityId = entityId;

            EntitiesVisible = new();
            EntitiesVisibleLast = new();
            Viewport = new RectangleF(0, 0, viewDistance, viewDistance);
            ChangedTick = NttWorld.Tick;
            Dirty = true;
        }

        public override int GetHashCode() => EntityId;
    }
}