using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Squiggly;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class DeathSystem : PixelSystem<DeathTagComponent>
    {
        public DeathSystem() : base("Death System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref DeathTagComponent dtc)
        {
            if (dtc.Tick == PixelWorld.Tick)
            {
                ref var eff = ref ntt.Get<StatusEffectComponent>();
                eff.Effects |= StatusEffect.Dead; 
                eff.Effects |= StatusEffect.FrozenRemoveName;

                if (ntt.Type == EntityType.Player)
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

                if (ntt.Type == EntityType.Monster)
                {
                    ref readonly var cqc = ref ntt.Get<CqActionComponent>();

                    long action = cqc.cq_Action;
                    while ((action = CqActionProcessor.Process(in ntt, in ntt, CqProcessor.GetAction(action))) != 0) ;

                    var drp = ntt.Get<DropComponent>();

                    if(drp.Drops.Money > 0 && Random.Shared.NextSingle() < 0.25f)
                    {
                        var drop = new RequestDropMoneyComponent(ntt.Id, Random.Shared.Next(drp.Drops.Money));
                        ntt.Set(ref drop);
                    }

                    var itemComp = ItemGenerator.Generate(drp.Drops);
                    if(itemComp.Id != 0)
                    {
                        var itemNtt = PixelWorld.CreateEntity(EntityType.Item);
                        itemNtt.Set(ref itemComp);
    
                        var rdi = new RequestDropItemComponent(ntt.Id, in itemNtt);
                        ntt.Set(ref rdi);
                    }
                }

                var effectsMsg = MsgUserAttrib.Create(ntt.NetId, (ulong)eff.Effects, MsgUserAttribType.StatusEffect);
                var deathMsg = MsgInteract.Create(in dtc.Killer, in ntt, MsgInteractType.Death, 0);
                ntt.NetSync(ref effectsMsg, true);
                ntt.NetSync(ref deathMsg, true);

                dtc.Killer.Remove<AttackComponent>();
                ntt.Remove<AttackComponent>();
                ntt.Remove<BrainComponent>();
            }
            else if (dtc.Tick + PixelWorld.TargetTps * 7 == PixelWorld.Tick && ntt.Type == EntityType.Monster)
            {
                ref var eff = ref ntt.Get<StatusEffectComponent>();
                eff.Effects |= StatusEffect.Fade;

                var update = MsgUserAttrib.Create(ntt.NetId, (ulong)eff.Effects, MsgUserAttribType.StatusEffect);
                ntt.NetSync(ref update, true);
            }
            else if (dtc.Tick + PixelWorld.TargetTps * 10 == PixelWorld.Tick && ntt.Type == EntityType.Monster)
            {
                var despwan = MsgAction.RemoveEntity(ntt.NetId);
                ntt.NetSync(ref despwan, true);
                var ded = new DestroyEndOfFrameComponent(ntt.Id);
                ntt.Set(ref ded);

                ref readonly var pos = ref ntt.Get<PositionComponent>();
                Game.Grids[pos.Map].Remove(in ntt);
            }
        }
    }
}