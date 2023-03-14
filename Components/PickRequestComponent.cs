using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component]
    public struct PickupRequestComponent
    {
        public NTT Item;

        public PickupRequestComponent(in NTT item) => Item = item;
    }
}