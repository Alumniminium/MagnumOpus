using MagnumOpus.ECS;
using Newtonsoft.Json;

namespace MagnumOpus.Components
{
    [Component]
    [Save]
    public struct ManaComponent
    {
        public long ChangedTick;
        public ushort Mana;
        public ushort MaxMana;

        [JsonConstructor]
        public ManaComponent(ushort mana, ushort maxMana)
        {
            ChangedTick = NttWorld.Tick;
            Mana = mana;
            MaxMana = maxMana;
        }
    }
}