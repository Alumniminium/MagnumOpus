using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
public struct MoneyRewardComponent(int amount)
{
    public int Amount = amount;
}