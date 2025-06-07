using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component(SaveEnabled: true)]
    public struct ReviveComponent(uint seconds)
    {
        public long ReviveTick = NttWorld.Tick + (seconds * NttWorld.TargetTps);
    }
}