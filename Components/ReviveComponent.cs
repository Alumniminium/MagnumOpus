using MagnumOpus.ECS;
namespace MagnumOpus.Components
{



    [Component(saveEnabled: true)]
    public struct ReviveComponent
    {
        public long ReviveTick;
        public ReviveComponent(uint seconds) => ReviveTick = NttWorld.Tick + (seconds * NttWorld.TargetTps);
    }
}