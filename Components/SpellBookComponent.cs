using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct SpellBookComponent
    {
        public Dictionary<ushort, (ushort lvl, ushort exp, ushort cooldown)> Spells;
        public SpellBookComponent() => Spells = [];
    }
}