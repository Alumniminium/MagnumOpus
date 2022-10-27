using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct LifeTimeComponent
    {
        public readonly int EntityId;
        public float LifeTimeSeconds;

        public LifeTimeComponent(int entityId, TimeSpan timespan)
        {
            EntityId = entityId;
            LifeTimeSeconds = (float)timespan.TotalSeconds;
        }
        public override int GetHashCode()
        {
            return EntityId;
        }
    }
}