using MagnumOpus.ECS;
using Newtonsoft.Json;

namespace MagnumOpus.Components.Items
{
    [Component]
    [Save]
    public readonly struct MoneyRewardComponent
    {
        public readonly int Amount;

        [JsonConstructor]
        public MoneyRewardComponent(int amount) => Amount = amount;
    }
}