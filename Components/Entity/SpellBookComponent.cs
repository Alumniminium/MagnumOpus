using MagnumOpus.ECS;
namespace MagnumOpus.Components.Entity
{
    [Component]
    [Save]
    public struct SpellBookComponent
    {
        public Dictionary<ushort, (ushort lvl, ushort exp, ushort cooldown)> Spells;
        public SpellBookComponent() => Spells = new();
    }
}