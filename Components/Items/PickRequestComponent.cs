using MagnumOpus.ECS;
using Newtonsoft.Json;

namespace MagnumOpus.Components.Items
{
    [Component]
    public readonly struct PickupRequestComponent
    {
        public readonly NTT Item;

        [JsonConstructor]
        public PickupRequestComponent(in NTT item) => Item = item;
    }
}