using System.Drawing;
using MagnumOpus.ECS;
using MagnumOpus.Helpers;
using Newtonsoft.Json;

namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct ViewportComponent
    {
        public long ChangedTick;
        public readonly ReaderWriterLockSlim rwLock = new();
        public HashSet<NTT> EntitiesVisible;
        public HashSet<NTT> EntitiesVisibleLast;
        private Rectangle _viewport;

        public Rectangle Viewport 
        { 
            readonly get => _viewport;
            set => NetworkSyncHelper.UpdateField(ref this, ref _viewport, value);
        }

        [JsonConstructor]
        public ViewportComponent()
        {
            ChangedTick = NttWorld.Tick;
            EntitiesVisible = new();
            EntitiesVisibleLast = new();
            _viewport = default;
        }
        
        public ViewportComponent(float viewDistance)
        {
            EntitiesVisible = new();
            EntitiesVisibleLast = new();
            _viewport = new Rectangle(0, 0, (int)viewDistance, (int)viewDistance);
            ChangedTick = NttWorld.Tick;
        }
    }
}