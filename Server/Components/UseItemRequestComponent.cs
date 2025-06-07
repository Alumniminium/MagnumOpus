using NttECS.ECS;

namespace MagnumOpus.Components;

[Component]
/// <summary>
/// Transient item usage request component that signals a player wants to consume or activate
/// an item from their inventory. Contains item network ID and usage parameter for context.
/// Not saved to database (no SaveEnabled). Processed by ItemUseSystem to validate item
/// ownership, execute item effects (health/mana restoration, action scripts), consume items,
/// and handle inventory updates. Essential for consumable items and item activation mechanics.
/// </summary>
public struct RequestItemUseComponent(int itemNetId, int param)
{
    public int ItemNetId = itemNetId;
    public int Param = param;
}