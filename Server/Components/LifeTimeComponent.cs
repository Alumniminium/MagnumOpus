using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct LifeTimeComponent(TimeSpan timespan)
    {
        public uint ExpireTick = (uint)(NttWorld.Tick + NttWorld.TargetTps * timespan.TotalSeconds);
    }
}