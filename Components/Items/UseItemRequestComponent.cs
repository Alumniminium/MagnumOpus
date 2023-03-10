using MagnumOpus.ECS;

namespace MagnumOpus.Components.Items
{
    [Component]
    public  struct RequestItemUseComponent
    {
        public  int ItemNetId;
        public  int Param;

        public RequestItemUseComponent(int itemNetId, int param)
        {
            ItemNetId = itemNetId;
            Param = param;
        }
    }
}