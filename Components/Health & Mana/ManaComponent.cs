using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    [Save]
    public struct ManaComponent
    {
        public readonly int EntityId;
        public long ChangedTick;
        public ushort Mana;
        public ushort MaxMana;

        public ManaComponent(int entityId, ushort mana, ushort maxMana)
        {
            EntityId = entityId;
            ChangedTick = NttWorld.Tick;
            Mana = mana;
            MaxMana = maxMana;
        }
        public override int GetHashCode() => EntityId;
    }
}