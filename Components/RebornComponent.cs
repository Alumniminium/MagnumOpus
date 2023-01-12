using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct RebornComponent
    {
        public readonly int EntityId;
        public long ChangedTick;
        
        public byte Count;

        public RebornComponent(int entityId)
        {
            EntityId = entityId;
            ChangedTick = PixelWorld.Tick;
            Count = 0;
        }

        public override int GetHashCode() => EntityId;
    }
}