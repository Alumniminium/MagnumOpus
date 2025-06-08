using System.Numerics;
using MagnumOpus.Components;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;
using NttECS.ECS;

namespace MagnumOpus.Systems
{
    public sealed class AttackSystem : NttSystem<AttackComponent, PositionComponent>
    {
        public AttackSystem() : base("Attack", threads: 1, log: false) { }

        // Handles combat mechanics including physical and ranged attacks. Manages attack timing,
        // distance validation, damage calculation, and target validation. Uses sleep ticks to
        // control attack speed and applies different damage multipliers for players and guards.
        public override void Update(in NTT ntt, ref AttackComponent atk, ref PositionComponent pos)
        {
            // === CHECK ATTACK COOLDOWN ===
            if (atk.CooldownTicks > 0)
            {
                atk.CooldownTicks--;
                return;
            }

            // === VALIDATE ATTACK DISTANCE ===
            ref readonly var targetPos = ref atk.Target.Get<PositionComponent>();
            var distance = Vector2.Distance(pos.Position, targetPos.Position);

            switch (atk.AttackType)
            {
                case MsgInteractType.Physical:
                    {
                        if (ntt.IsDead())
                        {
                            ntt.Remove<AttackComponent>();
                            break;
                        }
                        
                        // Physical attacks require close proximity (2.5 units)
                            if (distance > 2.5f)
                            {
                                ntt.Remove<AttackComponent>();
                                break;
                            }

                        // === CALCULATE AND APPLY PHYSICAL DAMAGE ===
                        atk.CooldownTicks = NttWorld.TargetTps; // 1 second attack cooldown

                        // TODO: Implement proper damage calculation system
                        var damage = Random.Shared.Next(1, 10);
                        if (ntt.IsPlayer())
                            damage *= 2;        // Players hit harder
                        if (ntt.Has<GuardPositionComponent>())
                            damage *= 10; // Guards are very strong

                        var damageComponent = new DamageComponent(in atk.Target, in ntt, damage);
                        atk.Target.Set(ref damageComponent);

                        var attackPacket = MsgInteract.Create(in ntt, in atk.Target, MsgInteractType.Physical, damage);
                        ntt.NetSync(ref attackPacket, broadcast: true);
                        break;
                    }

                case MsgInteractType.Ranged:
                    {
                        if (ntt.IsDead())
                        {
                            ntt.Remove<AttackComponent>();
                            break;
                        }
                        // Ranged attacks have longer range (10 units)
                        if (distance > 10)
                        {
                            ntt.Remove<AttackComponent>();
                            break;
                        }

                        // === CALCULATE AND APPLY RANGED DAMAGE ===
                        atk.CooldownTicks = NttWorld.TargetTps; // 1 second attack cooldown

                        // TODO: Implement proper damage calculation system
                        var damage = Random.Shared.Next(1, 10);
                        var damageComponent = new DamageComponent(in atk.Target, in ntt, damage);
                        atk.Target.Set(ref damageComponent);

                        // Ranged attacks show reduced, often half the damage ingame (client quirk, because it shoots more arrows than packets)
                        // so we divide damage by 2 so the healthbars show the correct amount
                        var attackPacket = MsgInteract.Create(in ntt, in atk.Target, MsgInteractType.Ranged, damage * 2);
                        ntt.NetSync(ref attackPacket, broadcast: true);
                        break;
                    }

                default:
                    throw new NotImplementedException("AttackType not implemented: " + atk.AttackType);
            }
        }
    }
}