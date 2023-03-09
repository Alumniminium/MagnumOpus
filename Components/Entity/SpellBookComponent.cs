using MagnumOpus.ECS;
using Newtonsoft.Json;

namespace MagnumOpus.Components.Entity
{
    [Component]
    [Save]
    public readonly struct SpellBookComponent
    {
        public readonly Dictionary<ushort, (ushort lvl, ushort exp, ushort cooldown)> Spells;

        [JsonConstructor]
        public SpellBookComponent() => Spells = new();
    }
}