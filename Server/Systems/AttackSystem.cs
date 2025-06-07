using System.Numerics;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;
using NttECS.ECS;

namespace MagnumOpus.Systems
{
    /// <summary>
    /// Handles combat mechanics including physical and ranged attacks.
    /// Manages attack timing, distance validation, damage calculation, and target validation.
    /// </summary>
    public sealed class AttackSystem : NttSystem<AttackComponent, PositionComponent>
    {
        /// <summary>
        /// Initializes the AttackSystem with half the available CPU cores for processing.
        /// </summary>
        public AttackSystem() : base("Attack", threads: 1) { }

        /// <summary>
        /// Processes an entity's attack against their target, handling damage and validation.
        /// </summary>
        /// <param name="ntt">The attacking entity</param>
        /// <param name="atk">Attack component containing target and attack configuration</param>
        /// <param name="pos">Attacker's position for distance calculations</param>
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

            ref readonly var targetPosition = ref atk.Target.Get<PositionComponent>();
            var distance = Vector2.Distance(pos.Position, targetPosition.Position);

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
                        if (ntt.Has<NetworkComponent>())
                            damage *= 2;
                        if (ntt.Has<GuardPositionComponent>())
                            damage *= 10;
                        var damageComponent = new DamageComponent(in atk.Target, in ntt, damage);
                        atk.Target.Set(ref damageComponent);
                        var attackPacket = MsgInteract.Create(in ntt, in atk.Target, MsgInteractType.Physical, damage);
                        ntt.NetSync(ref attackPacket, true);

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
                        var damageComponent = new DamageComponent(in atk.Target, in ntt, damage);
                        atk.Target.Set(ref damageComponent);

                        var attackPacket = MsgInteract.Create(in ntt, in atk.Target, MsgInteractType.Ranged, damage / 2);
                        ntt.NetSync(ref attackPacket, true);

                        break;
                    }
                default:
                    throw new NotImplementedException("AttackType not implemented: " + atk.AttackType);
            }
        }
    }
}