using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component]
    [Save]
    public struct MoneyRewardComponent
    {
        public int Amount;

        public MoneyRewardComponent(int amount) => Amount = amount;
    }
}