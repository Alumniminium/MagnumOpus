using MagnumOpus.ECS;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace MagnumOpus.Components
{
    [Component]
    public struct ManaComponent
    {
        public readonly int EntityId;
        public uint ChangedTick;
        public ushort Mana;
        public ushort MaxMana;

        public ManaComponent(int entityId, ushort mana, ushort maxMana)
        {
            EntityId = entityId;
            ChangedTick = PixelWorld.Tick;
            Mana = mana;
            MaxMana = maxMana;
        }
        public override int GetHashCode() => EntityId;
    }
}