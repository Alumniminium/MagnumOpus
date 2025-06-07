using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct ManaComponent(ushort mana, ushort maxMana)
    {
        public long ChangedTick = NttWorld.Tick;
        public ushort Mana = mana;
        public ushort MaxMana = maxMana;
    }
}