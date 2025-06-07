using NttECS.ECS;
namespace MagnumOpus.Components;

[Component]
/// <summary>
/// Transient pickup request component that signals a player wants to collect an item from
/// the ground. Contains reference to the target item entity to be picked up. Not saved to
/// database (no SaveEnabled). Processed by PickupSystem to validate inventory space, transfer
/// items or money to inventory, show pickup messages, and clean up ground entities.
/// </summary>
public struct PickupRequestComponent(in NTT item)
{
    public NTT Item = item;
}