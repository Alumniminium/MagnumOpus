using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct HealthComponent
    {
        public readonly int EntityId;
        public int Health;
        public int MaxHealth;

        public HealthComponent(int entityId, ushort health, ushort maxHealth)
        {
            EntityId = entityId;
            Health = health;
            MaxHealth = maxHealth;
        }
        public override int GetHashCode() => EntityId;
    }
}