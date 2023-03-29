using MagnumOpus.Components;
using MagnumOpus.ECS;

namespace MagnumOpus.Systems
{
    public sealed class DamageSystem : NttSystem<HealthComponent, DamageComponent>
    {
        public DamageSystem() : base("Damage", threads: 2) { }

        public override void Update(in NTT ntt, ref HealthComponent hlt, ref DamageComponent dmg)
        {
            var actualDamage = Math.Clamp(dmg.Damage, 0, hlt.Health);
            hlt.Health -= (ushort)actualDamage;

            if (hlt.Health <= 0)
            {
                hlt.Health = 0;
                var dtc = new DeathTagComponent(dmg.Attacker);
                ntt.Set(ref dtc);
                if (IsLogging)
                    Logger.Debug("{ntt} died after receiving {dmg} damage", ntt, dmg.Damage);
            }

            if (!dmg.Attacker.Has<ExpRewardComponent>())
            {
                var expReward = new ExpRewardComponent((ushort)actualDamage);
                dmg.Attacker.Set(ref expReward);
                if (IsLogging)
                    Logger.Debug("{attacker} received {exp} experience", dmg.Attacker, actualDamage);
            }
            else
            {
                ref var expReward = ref dmg.Attacker.Get<ExpRewardComponent>();
                expReward.Experience += (ushort)actualDamage;
            }
            ntt.Remove<DamageComponent>();
        }
    }
}