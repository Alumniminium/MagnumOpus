using NttECS.ECS;

namespace MagnumOpus.Components;

[Component]
/// <summary>
/// Transient request component for dropping money on the ground as a pickup item. Contains
/// the amount of money to drop. Not saved to database (no SaveEnabled). Processed by 
/// DropMoneySystem to create money entities on the ground that players can pick up. 
/// Typically triggered by player death or manual money dropping actions.
/// </summary>
public struct RequestDropMoneyComponent(int amount)
{
    public int Amount = amount;
}