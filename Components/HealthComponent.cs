using MagnumOpus.ECS;

namespace MagnumOpus.Simulation.Components
{
    [Component]
    public struct HealthComponent
    {
        public readonly int EntityId;
        public uint ChangedTick;
        public ushort Health;
        public ushort MaxHealth;

        public HealthComponent(int entityId, ushort health, ushort maxHealth)
        {
            EntityId = entityId;
            Health = health;
            MaxHealth = maxHealth;
            ChangedTick = Game.CurrentTick;
        }
        public override int GetHashCode()
        {
            return EntityId;
        }
    }
}