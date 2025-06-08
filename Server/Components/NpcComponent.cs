using MagnumOpus.Networking.Packets;
using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
/// <summary>
/// Non-Player Character component that identifies entities as NPCs with specific type data.
/// Contains Base ID, Type ID, and Sort classification for NPC behavior and appearance.
/// Used by extension methods for entity type checking (IsNpc()) and NPC system processing.
/// Links to database NPC definitions for scripted behaviors, dialogue, shops, and services.
/// Essential for interactive NPCs and game world population.
/// </summary>
public struct NpcComponent(ushort baseId, ushort typeId, ushort sort)
{
    public ushort Base = baseId;
    public NpcType Type = (NpcType)typeId;
    public NpcSort Sort = (NpcSort)sort;
}