using NttECS.ECS;
namespace MagnumOpus.Components
{
    [Component]
    public struct RequestDropItemComponent(in NTT itemNtt)
    {
        public NTT ItemNtt = itemNtt;
    }
}