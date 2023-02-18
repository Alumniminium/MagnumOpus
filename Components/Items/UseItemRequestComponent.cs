using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public readonly struct RequestItemUseComponent
    {
        public readonly int EntityId;
        public readonly int ItemNetId;
        public readonly int Param;

        public RequestItemUseComponent(int entityId, int itemNetId, int param)
        {
            EntityId = entityId;
            ItemNetId = itemNetId;
            Param = param;
        }

        public override int GetHashCode() => EntityId;
    }
}