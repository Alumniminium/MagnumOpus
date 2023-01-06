using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct DropRequestComponent
    {
        public readonly int EntityId;
        public int ItemNetId;

        public DropRequestComponent(int entityId, int itemNetId)
        {
            EntityId = entityId;
            ItemNetId = itemNetId;
        }
        
        public override int GetHashCode() => EntityId;
    }
}