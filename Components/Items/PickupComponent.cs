using MagnumOpus.ECS;
namespace MagnumOpus.Components.Items
{
    [Component]
    public readonly struct RequestDropItemComponent
    {
        public readonly NTT ItemNtt;

        public RequestDropItemComponent(in NTT itemNtt) => ItemNtt = itemNtt;
    }
}