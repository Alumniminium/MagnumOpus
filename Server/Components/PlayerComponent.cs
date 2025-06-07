using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
/// <summary>
/// Player entity marker component that identifies entities as human players. Empty struct that
/// serves purely as a tag for entity type classification. Used by extension methods for player
/// identification (IsPlayer()) and system filtering to distinguish players from NPCs, monsters,
/// and other entity types. Essential for player-specific logic, permissions, and interactions.
/// </summary>
public struct PlayerComponent
{
}