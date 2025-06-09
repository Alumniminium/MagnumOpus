using MagnumOpus.IO;
using MagnumOpus.Components;
using NttECS.ECS;

namespace MagnumOpus.Systems;

public sealed class DamageSystem : NttSystem<HealthComponent, DamageComponent>
{
    public DamageSystem() : base("Damage", threads: 1, log: false) { }

    // Applies damage to entities and handles the consequences. When an entity takes damage,
    // this system reduces their health and marks them for death if health reaches zero.
    // It also awards experience points to the attacker based on the actual damage dealt,
    // accumulating multiple damage instances into a single experience reward component.
    public override void Update(in NTT ntt, ref HealthComponent hlt, ref DamageComponent dmg)
    {
        // Skip processing damage on already dead entities
        if (ntt.Has<DeathTagComponent>())
        {
            ntt.Remove<DamageComponent>();
            return;
        }

        // === APPLY DAMAGE TO TARGET ===
        // Clamp damage to not exceed current health
        var actualDamage = Math.Clamp(dmg.Damage, 0, hlt.Health);
        hlt.Health -= (ushort)actualDamage;

        // Handle death if health reaches zero
        if (hlt.Health <= 0)
        {
            hlt.Health = 0;
            var deathTag = new DeathTagComponent(dmg.Attacker);
            ntt.Set(ref deathTag);

            if (IsLogging)
                FConsole.WriteLine("{ntt} died after receiving {dmg} damage", ntt, dmg.Damage);
        }

        // === AWARD EXPERIENCE TO ATTACKER ===
        // Add to existing experience component or create new one
        if (!dmg.Attacker.Has<ExpRewardComponent>())
        {
            var experienceReward = new ExpRewardComponent((ushort)actualDamage);
            dmg.Attacker.Set(ref experienceReward);

            if (IsLogging)
                FConsole.WriteLine("{attacker} received {exp} experience", dmg.Attacker, actualDamage);
        }
        else
        {
            // Accumulate damage into existing experience reward
            ref var experienceReward = ref dmg.Attacker.Get<ExpRewardComponent>();
            experienceReward.Experience += (ushort)actualDamage;
        }

        // Clean up the damage component
        ntt.Remove<DamageComponent>();
    }
}