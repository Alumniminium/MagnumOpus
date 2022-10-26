using MagnumOpus.ECS;

namespace MagnumOpus.Simulation.Components
{
    [Component]
    public struct RebornComponent
    {
        public readonly int EntityId;
        public uint ChangedTick;
        
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