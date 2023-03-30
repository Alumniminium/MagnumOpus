using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct MoneyRewardComponent
    {
        public int Amount;

        public MoneyRewardComponent(int amount) => Amount = amount;
    }
}