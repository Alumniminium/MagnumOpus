using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public partial struct RebornComponent
    {
        public long ChangedTick = NttWorld.Tick;
        private byte _count;

        public RebornComponent() { }
    }
}