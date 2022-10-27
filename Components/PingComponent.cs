using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct PingComponent
    {
        public readonly int EntityId;
        public int LastPing;

        public int Ping;

        public PingComponent(int entityId)
        {
            EntityId = entityId;
            LastPing = 0;
            Ping = 0;
        }

        public override int GetHashCode() => EntityId;        
    }
}