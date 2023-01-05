using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class DeathSystem : PixelSystem<DeathTagComponent>
    {
        public DeathSystem() : base("Death System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref DeathTagComponent dtc)
        {
            var death = MsgInteract.Create(in dtc.Killer, in ntt, MsgInteractType.Death, 0);
            // var despwan = MsgAction.RemoveEntity(ntt.NetId);
            // ntt.NetSync(ref despwan, true);
            ntt.NetSync(ref death, true);

            ref var eff = ref ntt.Get<StatusEffectComponent>();
            eff.Effects |= StatusEffect.Dead;

            if(ntt.Type == EntityType.Player)
            {
                ref var bdy = ref ntt.Get<BodyComponent>();
                uint ghostLook = 0;
                if (bdy.Look % 10000 == 2001 || bdy.Look % 10000 == 2002)
                    ghostLook = MsgSpawn.AddTransform(bdy.Look, 99);
                else
                    ghostLook = MsgSpawn.AddTransform(bdy.Look, 98);

                var msgUpdate = MsgUserAttrib.Create(ntt.NetId, ghostLook, MsgUserAttribType.Look);
                ntt.NetSync(ref msgUpdate, true);
            }

            if(ntt.Type == EntityType.Monster)
            {
                eff.Effects |= StatusEffect.Fade;
                ref readonly var cqc = ref ntt.Get<CqActionComponent>();
                
                long action = cqc.cq_Action;
                while((action = CqActionProcessor.Process(in ntt, CqProcessor.GetAction(action))) != 0);
            }

            var update = MsgUserAttrib.Create(ntt.NetId, (ulong)eff.Effects, MsgUserAttribType.StatusEffect);
            ntt.NetSync(ref update, true);

            dtc.Killer.Remove<AttackComponent>();
            ntt.Remove<AttackComponent>();
            
            ref readonly var pos = ref ntt.Get<PositionComponent>();
            Game.Grids[pos.Map].Remove(in ntt);
            
            if(ntt.Type != EntityType.Player)
                PixelWorld.Destroy(in ntt);
            else
                ntt.Remove<DeathTagComponent>();
        }
    }
}