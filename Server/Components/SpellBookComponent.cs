using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
public struct SpellBookComponent
{
    public Dictionary<ushort, (ushort lvl, ushort exp, ushort cooldown)> Spells;
    public SpellBookComponent() => Spells = [];
}