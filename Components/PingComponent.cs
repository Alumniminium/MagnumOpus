using MagnumOpus.ECS;

namespace MagnumOpus.Simulation.Components
{
    [Component]
    public struct PingComponent
    {
        public readonly int EntityId;
        public int LastPing;

        public PingComponent(int entityId)
        {
            EntityId = entityId;
            LastPing = 0;
        }

        public override int GetHashCode() => EntityId;        
    }
}