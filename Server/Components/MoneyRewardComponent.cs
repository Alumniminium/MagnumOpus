using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct MoneyRewardComponent(int amount)
    {
        public int Amount = amount;
    }
}