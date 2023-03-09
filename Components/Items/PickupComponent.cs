using MagnumOpus.ECS;
using Newtonsoft.Json;

namespace MagnumOpus.Components.Items
{
    [Component]
    public readonly struct RequestDropItemComponent
    {
        public readonly NTT ItemNtt;

        [JsonConstructor]
        public RequestDropItemComponent(in NTT itemNtt) => ItemNtt = itemNtt;
    }
}