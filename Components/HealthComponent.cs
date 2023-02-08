using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct HealthComponent
    {
        public readonly int EntityId;
        public int Health;
        public int MaxHealth;
        internal long ChangedTick;

        public HealthComponent(int entityId, ushort health, ushort maxHealth)
        {
            EntityId = entityId;
            Health = health;
            MaxHealth = maxHealth;
            ChangedTick = NttWorld.Tick;
        }
        public override int GetHashCode() => EntityId;
    }
}