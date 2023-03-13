using MagnumOpus.ECS;
namespace MagnumOpus.Components.Death
{
    [Component]
    [Save]
    public struct ReviveComponent
    {
        public long ReviveTick;
        public ReviveComponent(uint seconds) => ReviveTick = NttWorld.Tick + seconds * NttWorld.TargetTps;
    }
}