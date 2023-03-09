using MagnumOpus.ECS;

namespace MagnumOpus.Components.Attack
{
    [Component]
    public struct DamageComponent
    {
        public readonly int EntityId;
        public readonly NTT Attacker;
        public int Damage;

        public DamageComponent(in NTT attacked, in NTT attacker, int damage)
        {
            EntityId = attacked.Id;
            Attacker = attacker;
            Damage = damage;
        }
        public override int GetHashCode() => EntityId;
    }
}