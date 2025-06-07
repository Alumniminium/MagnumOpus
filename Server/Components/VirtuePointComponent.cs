using MagnumOpus.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
public struct VirtuePointComponent(long points)
{
    public long ChangedTick = NttWorld.Tick;
    public long Points = points;
}