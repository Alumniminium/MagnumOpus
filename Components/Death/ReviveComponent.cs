using MagnumOpus.ECS;
namespace MagnumOpus.Components.Death
{
    [Component]
    [Save]
    public readonly struct ReviveComponent
    {
        public readonly long ReviveTick;
        public ReviveComponent(uint seconds) => ReviveTick = NttWorld.Tick + seconds * NttWorld.TargetTps;
    }
}