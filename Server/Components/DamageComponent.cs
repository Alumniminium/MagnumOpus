using NttECS.ECS;
namespace MagnumOpus.Components
{
    [Component]
    /// <summary>
    /// Transient damage event component that represents damage dealt from one entity to another.
    /// Contains the attacker, target, and damage amount. Not saved to database (no SaveEnabled).
    /// Processed by DamageSystem to apply health reduction, death checks, and cleanup. Created
    /// by AttackSystem and MagicAttackSystem when attacks connect with their targets.
    /// </summary>
    public struct DamageComponent(in NTT attacked, in NTT attacker, int damage)
    {
        public NTT Attacker = attacker;
        public NTT Attacked = attacked;
        public int Damage = damage;
    }
}