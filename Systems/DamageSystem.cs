using MagnumOpus.ECS;
using MagnumOpus.Components;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class DamageSystem : PixelSystem<HealthComponent, DamageComponent>
    {
        public DamageSystem() : base("Damage System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref HealthComponent hlt, ref DamageComponent dmg)
        {
            hlt.Health -= (ushort)dmg.Damage;

            if(hlt.Health <= 0)
            {
                hlt.Health = 0;
                var dtc = new DeathTagComponent(ntt.Id, dmg.Attacker);
                ntt.Add(ref dtc);
            }

            ntt.Remove<DamageComponent>();
            var expReward = new ExpRewardComponent(in dmg.Attacker, (ushort)dmg.Damage);
            dmg.Attacker.Add(ref expReward);

            var healthUpdate = MsgUserAttrib.Create(ntt.NetId, (ushort)hlt.Health, Enums.MsgUserAttribType.Health);
            ntt.NetSync(ref healthUpdate, true);
        }
    }
}