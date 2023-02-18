using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public readonly struct SpellBookComponent
    {
        public readonly int EntityId;
        public readonly Dictionary<ushort,(ushort lvl, ushort exp, ushort cooldown)> Spells;

        public SpellBookComponent(int id)
        {
            EntityId = id;
            Spells = new ();
        }

        public override int GetHashCode() => EntityId;
    }
}