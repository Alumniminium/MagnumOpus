using System.Numerics;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class AttackSystem : NttSystem<AttackComponent, PositionComponent>
    {
        public AttackSystem() : base("Attack", threads: 2) { }

        public override void Update(in NTT ntt, ref AttackComponent atk, ref PositionComponent pos)
        {
            if (atk.SleepTicks > 0)
            {
                atk.SleepTicks--;
                return;
            }

            if (atk.Target.Has<DeathTagComponent>())
            {
                ntt.Remove<AttackComponent>();
                return;
            }

            // TODO: Implement
            ref readonly var targetPos = ref atk.Target.Get<PositionComponent>();

            var distance = Vector2.Distance(pos.Position, targetPos.Position);

            if (atk.AttackType == MsgInteractType.Physical)
            {
                if (distance <= 2.5f)
                {
                    atk.SleepTicks = NttWorld.TargetTps;
                    // TODO: calculate damage

                    var damage = Random.Shared.Next(1, 10);
                    if (ntt.Type == EntityType.Player)
                        damage *= 2;
                    if (ntt.Has<GuardPositionComponent>())
                        damage *= 10;

                    var dmg = new DamageComponent(in atk.Target, in ntt, damage);
                    atk.Target.Set(ref dmg);

                    var atkPacket = MsgInteract.Create(in ntt, in atk.Target, MsgInteractType.Physical, damage);
                    ntt.NetSync(ref atkPacket, true);
                }
                else
                    ntt.Remove<AttackComponent>();
            }
            else if (atk.AttackType == MsgInteractType.Archer)
            {
                if (distance <= 10)
                {
                    atk.SleepTicks = NttWorld.TargetTps;
                    // TODO: calculate damage

                    var damage = Random.Shared.Next(1, 10);
                    var dmg = new DamageComponent(in atk.Target, in ntt, damage);
                    atk.Target.Set(ref dmg);

                    var atkPacket = MsgInteract.Create(in ntt, in atk.Target, MsgInteractType.Archer, damage/2);
                    ntt.NetSync(ref atkPacket, true);
                }
                else
                    ntt.Remove<AttackComponent>();
            }
            else
                throw new NotImplementedException("AttackType not implemented: " + atk.AttackType);
        }
    }
}