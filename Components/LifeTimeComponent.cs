using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct LifeTimeComponent
    {
        public uint ExpireTick;

        public LifeTimeComponent(TimeSpan timespan) => ExpireTick = (uint)(NttWorld.Tick + NttWorld.TargetTps * timespan.TotalSeconds);
    }
}