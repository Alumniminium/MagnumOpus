using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct DropRequestComponent
    {
        public readonly int EntityId;
        public byte ItemNetId;

        public DropRequestComponent(int entityId, int itemNetId)
        {
            EntityId = entityId;
            ItemNetId = (byte)itemNetId;
        }
        
        public override int GetHashCode() => EntityId;
    }
}