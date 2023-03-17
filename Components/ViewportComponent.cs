using System.Collections.Concurrent;
using System.Drawing;
using MagnumOpus.ECS;
using Newtonsoft.Json;

namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct ViewportComponent
    {
        public long ChangedTick;

        public ConcurrentDictionary<int, NTT> EntitiesVisible;
        public ConcurrentDictionary<int, NTT> EntitiesVisibleLast;
        public RectangleF Viewport;
        internal bool Dirty;

        [JsonConstructor]
        public ViewportComponent()
        {
            ChangedTick = NttWorld.Tick;
            EntitiesVisible = new();
            EntitiesVisibleLast = new();
            Dirty = true;
        }
        public ViewportComponent(float viewDistance)
        {
            EntitiesVisible = new();
            EntitiesVisibleLast = new();
            Viewport = new RectangleF(0, 0, viewDistance, viewDistance);
            ChangedTick = NttWorld.Tick;
            Dirty = true;
        }
    }
}