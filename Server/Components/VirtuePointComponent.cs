using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public partial struct VirtuePointComponent
    {
        public long ChangedTick = NttWorld.Tick;
        private long _points;

        public VirtuePointComponent() { }
        public VirtuePointComponent(long points)
        {
            Points = points;  // Uses generated property
        }
    }
}