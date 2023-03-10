using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component]
    [Save]
    public struct ManaComponent
    {
        public long ChangedTick;
        public ushort Mana;
        public ushort MaxMana;

        public ManaComponent() => ChangedTick = NttWorld.Tick;
        public ManaComponent(ushort mana, ushort maxMana)
        {
            ChangedTick = NttWorld.Tick;
            Mana = mana;
            MaxMana = maxMana;
        }
    }
}