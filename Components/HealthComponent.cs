using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct HealthComponent
    {
        public readonly int EntityId;
        public uint ChangedTick;
        public int Health;
        public int MaxHealth;

        public HealthComponent(int entityId, ushort health, ushort maxHealth)
        {
            EntityId = entityId;
            Health = health;
            MaxHealth = maxHealth;
            ChangedTick = PixelWorld.Tick;
        }
        public override int GetHashCode()
        {
            return EntityId;
        }
    }
}