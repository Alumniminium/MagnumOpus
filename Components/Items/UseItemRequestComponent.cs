using MagnumOpus.ECS;

namespace MagnumOpus.Components.Items
{
    [Component]
    public readonly struct RequestItemUseComponent
    {
        public readonly int ItemNetId;
        public readonly int Param;

        public RequestItemUseComponent(int itemNetId, int param)
        {
            ItemNetId = itemNetId;
            Param = param;
        }
    }
}