using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component]
    public struct RequestDropItemComponent
    {
        public NTT ItemNtt;

        public RequestDropItemComponent(in NTT itemNtt) => ItemNtt = itemNtt;
    }
}