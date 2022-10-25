using MagnumOpus.ECS;

namespace MagnumOpus.Simulation.Components
{
    [Component]
    public struct DamageComponent
    {
        public readonly int EntityId;
        public readonly int AttackerId;
        public float Damage;

        public DamageComponent(int entityId, int attackerId, float damage)
        {
            EntityId = entityId;
            AttackerId = attackerId;
            Damage = damage;
        }
        public override int GetHashCode()
        {
            return EntityId;
        }
    }
}