using MagnumOpus.ECS;
namespace MagnumOpus.Components.Items
{
    [Component]
    public readonly struct PickupRequestComponent
    {
        public readonly NTT Item;

        public PickupRequestComponent(in NTT item) => Item = item;
    }
}