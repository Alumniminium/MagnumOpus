using MagnumOpus.ECS;
namespace MagnumOpus.Components.Entity
{
    [Component]
    [Save]
    public readonly struct SpellBookComponent
    {
        public readonly Dictionary<ushort, (ushort lvl, ushort exp, ushort cooldown)> Spells;

        public SpellBookComponent() => Spells = new();
    }
}