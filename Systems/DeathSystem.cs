using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class DeathSystem : NttSystem<DeathTagComponent>
    {
        public DeathSystem() : base("Death", threads: 1) { }

        public override void Update(in NTT ntt, ref DeathTagComponent dtc)
        {
            if (dtc.Tick == NttWorld.Tick)
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

                // if (ntt.Has<CqActionComponent>())
                // {
                //     ref readonly var cqc = ref ntt.Get<CqActionComponent>();
                //     long action = cqc.cq_Action;
                //     for(int i =0; i < 32; i++)
                //     {
                //         if (action == 0)
                //             break;
                //         action = CqActionProcessor.Process(in ntt, in ntt, CqProcessor.GetAction(action));
                //     }
                // }
                if(ntt.Has<InventoryComponent>())
                {
                    ref var inv = ref ntt.Get<InventoryComponent>();

                    if (inv.Money > 0 && Random.Shared.NextSingle() < 0.25f)
                    {
                        var drop = new RequestDropMoneyComponent(ntt.Id, Random.Shared.Next((int)inv.Money));
                        ntt.Set(ref drop);
                    }

                    if(ntt.Type == EntityType.Monster)
                    {
                        var cqm = ntt.Get<CqMonsterComponent>();
                        var items = ItemGenerator.GetDropItemsFor(cqm.CqMonsterId);
                        for (int i = 0; i < items.Count; i++)
                        {
                            var item = items[i];
                            ref var invItemNtt = ref EntityFactory.MakeDefaultItem(item.ID, out var success, default, 0, true);
                            inv.Items[i] = invItemNtt;
                        }
                    }
                    
                    inv.Items = inv.Items.OrderByDescending(x => x.Id).ToArray();
                    var itemCount = inv.Items.Where(x => x.Id != 0).Count();
                    for (int i = 0; i < itemCount; i++)
                    {
                        if (Random.Shared.NextSingle() >= 0.1f)
                            continue;

                        ref var itemComp = ref inv.Items[i].Get<ItemComponent>();
                        if (itemComp.Id == 0)
                            continue;
                        var rdi = new RequestDropItemComponent(ntt.Id, in inv.Items[i]);
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
                ntt.Remove<WalkComponent>();
                ntt.Remove<JumpComponent>();
            }
            else if (dtc.Tick + NttWorld.TargetTps * 7 == NttWorld.Tick && ntt.Type == EntityType.Monster)
            {
                ref var eff = ref ntt.Get<StatusEffectComponent>();
                eff.Effects |= StatusEffect.Fade;

                var update = MsgUserAttrib.Create(ntt.NetId, (ulong)eff.Effects, MsgUserAttribType.StatusEffect);
                ntt.NetSync(ref update, true);
            }
            else if (dtc.Tick + NttWorld.TargetTps * 10 == NttWorld.Tick && ntt.Type == EntityType.Monster)
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