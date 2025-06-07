using NttECS.ECS;
namespace MagnumOpus.Components
{
    [Component(SaveEnabled: true)]
    public struct LifeTimeComponent(TimeSpan timespan)
    {
        public uint ExpireTick = (uint)(NttWorld.Tick + NttWorld.TargetTps * timespan.TotalSeconds);
    }
}