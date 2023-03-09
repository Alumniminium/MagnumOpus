using MagnumOpus.ECS;

namespace MagnumOpus.Components.Entity
{
    [Component]
    [Save]
    public readonly struct SpellBookComponent
    {
        public readonly int EntityId;
        public readonly Dictionary<ushort, (ushort lvl, ushort exp, ushort cooldown)> Spells = new();

        public SpellBookComponent(int id) => EntityId = id;

        public override int GetHashCode() => EntityId;
    }
}