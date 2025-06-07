using NttECS.ECS;

namespace MagnumOpus.Components;

[Component]
/// <summary>
/// Viewport refresh marker component that signals an entity needs its visible entity list
/// updated. Empty readonly struct used as a tag to trigger viewport recalculation. Not saved
/// to database (no SaveEnabled). Used by ViewportSystem to refresh visibility, and extensively
/// by AI systems, TeleportSystem, ReviveSystem, WalkSystem, and others when entities change
/// position or need fresh spatial awareness. Essential for maintaining accurate entity visibility.
/// </summary>
public readonly struct ViewportUpdateTagComponent
{
}
