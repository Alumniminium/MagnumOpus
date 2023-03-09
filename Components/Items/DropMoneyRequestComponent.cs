using MagnumOpus.ECS;
using Newtonsoft.Json;

namespace MagnumOpus.Components.Items
{
    [Component]
    public readonly struct RequestDropMoneyComponent
    {
        public readonly int Amount;

        [JsonConstructor]
        public RequestDropMoneyComponent(int amount) => Amount = amount;
    }
}