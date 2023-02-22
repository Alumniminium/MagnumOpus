using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component][Save]
    public struct LifeTimeComponent
    {
        public readonly int EntityId;
        public uint ExpireTick;

        public LifeTimeComponent(int entityId, TimeSpan timespan)
        {
            EntityId = entityId;
            ExpireTick = (uint)(NttWorld.Tick + (NttWorld.TargetTps * timespan.TotalSeconds));
        }
        public override int GetHashCode() => EntityId;
    }
}