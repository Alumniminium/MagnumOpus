using MagnumOpus.ECS;
namespace MagnumOpus.Components.Attack
{
    [Component]
    [Save]
    public struct DamageComponent
    {
        public readonly NTT Attacker;
        public readonly NTT Attacked;
        public int Damage;

        public DamageComponent(in NTT attacked, in NTT attacker, int damage)
        {
            Attacked = attacked;
            Attacker = attacker;
            Damage = damage;
        }
    }
}