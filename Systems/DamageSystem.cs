using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class DamageSystem : PixelSystem<HealthComponent, DamageComponent>
    {
        public DamageSystem() : base("Damage System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref HealthComponent hlt, ref DamageComponent dmg)
        {
            var actualDamage = Math.Clamp(dmg.Damage, 0,hlt.Health);
            hlt.Health -= (ushort)actualDamage;

            if(hlt.Health <= 0)
            {
                hlt.Health = 0;
                var dtc = new DeathTagComponent(ntt.Id, dmg.Attacker);
                ntt.Add(ref dtc);
            }

            var expReward = new ExpRewardComponent(in dmg.Attacker, (ushort)actualDamage);
            dmg.Attacker.Add(ref expReward);

            var healthUpdate = MsgUserAttrib.Create(ntt.NetId, (ushort)hlt.Health, Enums.MsgUserAttribType.Health);
            ntt.NetSync(ref healthUpdate, true);
            ntt.Remove<DamageComponent>();
        }
    }
}