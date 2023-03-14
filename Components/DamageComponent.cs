using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component]
    public struct DamageComponent
    {
        public NTT Attacker;
        public NTT Attacked;
        public int Damage;

        public DamageComponent(in NTT attacked, in NTT attacker, int damage)
        {
            Attacked = attacked;
            Attacker = attacker;
            Damage = damage;
        }
    }
}