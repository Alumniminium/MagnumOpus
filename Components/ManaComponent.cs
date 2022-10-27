using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct ManaComponent
    {
        public readonly int EntityId;
        public uint ChangedTick;
        public ushort Mana;
        public ushort MaxMana;

        public ManaComponent(int entityId)
        {
            EntityId = entityId;
            ChangedTick = 0;
            Mana = 0;
            MaxMana = 0;
        }
        public override int GetHashCode()
        {
            return EntityId;
        }
    }
}