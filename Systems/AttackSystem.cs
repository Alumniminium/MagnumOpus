using System.Numerics;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Systems
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

            switch (atk.AttackType)
            {
                case MsgInteractType.Physical:
                    {
                        if (distance > 2.5f)
                        {
                            ntt.Remove<AttackComponent>();
                            break;
                        }

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

                        break;
                    }
                case MsgInteractType.Ranged:
                    {
                        if (distance > 10)
                        {
                            ntt.Remove<AttackComponent>();
                            break;
                        }

                        atk.SleepTicks = NttWorld.TargetTps;
                        // TODO: calculate damage
                        var damage = Random.Shared.Next(1, 10);
                        var dmg = new DamageComponent(in atk.Target, in ntt, damage);
                        atk.Target.Set(ref dmg);

                        var atkPacket = MsgInteract.Create(in ntt, in atk.Target, MsgInteractType.Ranged, damage / 2);
                        ntt.NetSync(ref atkPacket, true);

                        break;
                    }
                default:
                    throw new NotImplementedException("AttackType not implemented: " + atk.AttackType);
            }
        }
    }
}