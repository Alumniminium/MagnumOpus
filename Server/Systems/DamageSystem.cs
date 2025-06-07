using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using NttECS.ECS;

namespace MagnumOpus.Systems
{
    /// <summary>
    /// Processes damage against entities, updating health and handling death conditions.
    /// Also manages experience rewards for attackers based on damage dealt.
    /// </summary>
    public sealed class DamageSystem : NttSystem<HealthComponent, DamageComponent>
    {
        /// <summary>
        /// Initializes the DamageSystem with half the available CPU cores for processing.
        /// </summary>
        public DamageSystem() : base("Damage", threads: 1) { }

        /// <summary>
        /// Applies damage to an entity, handles death conditions, and awards experience to attackers.
        /// </summary>
        /// <param name="ntt">The entity receiving damage</param>
        /// <param name="hlt">Health component of the damaged entity</param>
        /// <param name="dmg">Damage component containing damage amount and attacker information</param>
        public override void Update(in NTT ntt, ref HealthComponent hlt, ref DamageComponent dmg)
        {
            var actualDamage = Math.Clamp(dmg.Damage, 0, hlt.Health);
            hlt.Health -= (ushort)actualDamage;

            if (hlt.Health <= 0)
            {
                hlt.Health = 0;
                var deathTag = new DeathTagComponent(dmg.Attacker);
                ntt.Set(ref deathTag);
                if (IsLogging)
                    FConsole.WriteLine("{ntt} died after receiving {dmg} damage", ntt, dmg.Damage);
            }

            if (!dmg.Attacker.Has<ExpRewardComponent>())
            {
                var experienceReward = new ExpRewardComponent((ushort)actualDamage);
                dmg.Attacker.Set(ref experienceReward);
                if (IsLogging)
                    FConsole.WriteLine("{attacker} received {exp} experience", dmg.Attacker, actualDamage);
            }
            else
            {
                ref var experienceReward = ref dmg.Attacker.Get<ExpRewardComponent>();
                experienceReward.Experience += (ushort)actualDamage;
            }
            ntt.Remove<DamageComponent>();
        }
    }
}