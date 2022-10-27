using MagnumOpus.ECS;
using MagnumOpus.Simulation.Components;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class DamageSystem : PixelSystem<HealthComponent, DamageComponent>
    {
        public DamageSystem() : base("Damage System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref HealthComponent hlt, ref DamageComponent dmg)
        {
            if (ntt.Has<RespawnTagComponent>())
                return;
            if (!ConquerWorld.EntityExists(dmg.AttackerId))
                return;
            var attacker = ConquerWorld.GetEntity(dmg.AttackerId);
           
            if (dmg.Damage > 0)
            {
                var rewardableDamage = Math.Min(dmg.Damage, hlt.Health);
                hlt.Health -= (ushort)Math.Clamp(hlt.Health, 0, dmg.Damage);
                hlt.ChangedTick = ConquerWorld.Tick;

                if (attacker.Has<LevelComponent>())
                {
                    var exp = new ExpRewardComponent(ntt.Id, (uint)rewardableDamage);
                    attacker.Add(ref exp);
                }

                if (hlt.Health <= 0)
                {
                    var dtc = new DeathTagComponent(ntt.Id, dmg.AttackerId);
                    ntt.Add(ref dtc);
                }
            }
            ntt.Remove<DamageComponent>();
        }
    }
}