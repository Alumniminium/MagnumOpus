using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct RequestItemUseComponent(int itemNetId, int param)
    {
        public int ItemNetId = itemNetId;
        public int Param = param;
    }
}