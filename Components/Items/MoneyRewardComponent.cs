using MagnumOpus.ECS;
namespace MagnumOpus.Components.Items
{
    [Component]
    [Save]
    public readonly struct MoneyRewardComponent
    {
        public readonly int Amount;

        public MoneyRewardComponent(int amount) => Amount = amount;
    }
}