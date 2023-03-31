using System.Drawing;
using MagnumOpus.ECS;
using Newtonsoft.Json;

namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct ViewportComponent
    {
        public long ChangedTick;
        public readonly ReaderWriterLockSlim rwLock = new();
        public Dictionary<int, NTT> EntitiesVisible;
        public Dictionary<int, NTT> EntitiesVisibleLast;
        public Rectangle Viewport;
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
            Viewport = new Rectangle(0, 0, (int)viewDistance, (int)viewDistance);
            ChangedTick = NttWorld.Tick;
            Dirty = true;
        }
    }
}