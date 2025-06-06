using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component]
    public struct DamageComponent(in NTT attacked, in NTT attacker, int damage)
    {
        public NTT Attacker = attacker;
        public NTT Attacked = attacked;
        public int Damage = damage;
    }
}