using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct BodyComponent
    {
        public readonly int EntityId;
        public long ChangedTick;
        
        public uint Look;

        public BodyComponent(int entityId, uint look = 1003)
        {
            EntityId = entityId;
            ChangedTick = PixelWorld.Tick;
            Look = look;
        }

        public override int GetHashCode() => EntityId;
    }
}
