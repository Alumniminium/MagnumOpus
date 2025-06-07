using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
/// <summary>
/// Player spellbook component managing learned spells and magic abilities. Contains dictionary
/// mapping spell IDs to spell data including level, experience, and cooldown information.
/// Currently defined but not actively processed by any systems - represents planned magic
/// system for spell learning, progression, cooldown management, and magical ability access
/// based on character development and spell acquisition.
/// </summary>
public struct SpellBookComponent
{
    public Dictionary<ushort, (ushort lvl, ushort exp, ushort cooldown)> Spells;
    public SpellBookComponent() => Spells = [];
}