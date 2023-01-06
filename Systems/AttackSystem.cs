using MagnumOpus.ECS;
using MagnumOpus.Components;
using MagnumOpus.Enums;
using System.Numerics;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class AttackSystem : PixelSystem<AttackComponent, PositionComponent>
    {
        public AttackSystem() : base("Attack System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref AttackComponent atk, ref PositionComponent pos)
        {
            if(atk.SleepTicks > 0)
            {
                atk.SleepTicks--;
                return;
            }

            // TODO: Implement
            ref readonly var targetPos = ref atk.Target.Get<PositionComponent>();

            var distance = Vector2.Distance(pos.Position, targetPos.Position);
            
            if(atk.AttackType == MsgInteractType.Physical)
            {
                if(distance <= 2.5f)
                {
                    atk.SleepTicks = PixelWorld.TargetTps;
                    // TODO: calculate damage

                    var damage = Random.Shared.Next(1,10);
                    if(ntt.Type == EntityType.Player)
                        damage *= 2;
                    if(ntt.Has<GuardComponent>())
                        damage *= 10;

                    var dmg = new DamageComponent(in atk.Target, in ntt, damage);
                    atk.Target.Add(ref dmg);

                    var atkPacket = MsgInteract.Create(in ntt, in atk.Target, MsgInteractType.Physical, damage);
                    ntt.NetSync(ref atkPacket, true);
                }
                else
                    ntt.Remove<AttackComponent>();
            }
            else if(atk.AttackType == MsgInteractType.Archer)
            {
                if(distance <= 10)
                {
                    atk.SleepTicks = PixelWorld.TargetTps;
                    // TODO: calculate damage

                    var damage = Random.Shared.Next(1,10);
                    var dmg = new DamageComponent(in atk.Target, in ntt, damage);
                    atk.Target.Add(ref dmg);

                    var atkPacket = MsgInteract.Create(in ntt, in atk.Target, MsgInteractType.Archer, damage);
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