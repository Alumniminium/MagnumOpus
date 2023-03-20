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
        public object dictLock = new();
        public readonly ReaderWriterLockSlim rwLock = new();
        public Dictionary<int, NTT> EntitiesVisible;
        public Dictionary<int, NTT> EntitiesVisibleLast;
        public RectangleF Viewport;
        internal bool Dirty;

        [JsonConstructor]
        public ViewportComponent()
        {
            ChangedTick = NttWorld.Tick;
            EntitiesVisible = new();
            EntitiesVisibleLast = new();
            Dirty = true;
            dictLock = new();
        }
        public ViewportComponent(float viewDistance)
        {
            EntitiesVisible = new();
            EntitiesVisibleLast = new();
            Viewport = new RectangleF(0, 0, viewDistance, viewDistance);
            ChangedTick = NttWorld.Tick;
            Dirty = true;
            dictLock = new();
        }
    }
}