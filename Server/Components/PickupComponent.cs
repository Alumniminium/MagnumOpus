using NttECS.ECS;

namespace MagnumOpus.Components;

[Component]
/// <summary>
/// Transient item drop request component that signals a player wants to drop an item from
/// their inventory to the ground. Contains reference to the item entity to be dropped.
/// Not saved to database (no SaveEnabled). Processed by DropItemSystem to validate inventory
/// ownership, remove item from inventory, create ground entity with 30-second lifetime,
/// and broadcast to nearby players. Used by DeathSystem for death item drops.
/// </summary>
public struct RequestDropItemComponent(in NTT itemNtt)
{
    public NTT ItemNtt = itemNtt;
}