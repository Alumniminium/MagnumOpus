using NttECS.ECS;

namespace MagnumOpus.Components;

[Component]
public struct RequestDropMoneyComponent(int amount)
{
    public int Amount = amount;
}