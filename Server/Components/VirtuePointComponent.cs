using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct VirtuePointComponent(long points)
    {
        public long ChangedTick = NttWorld.Tick;
        public long Points = points;
    }
}