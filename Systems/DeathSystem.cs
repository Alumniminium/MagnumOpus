using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Systems
{
    public sealed class DeathSystem : NttSystem<DeathTagComponent>
    {
        public DeathSystem() : base("Death", threads: 2) { }

        public override void Update(in NTT ntt, ref DeathTagComponent dtc)
        {
            if (ntt.Type is EntityType.Player or EntityType.Npc or EntityType.Monster)
                EntityDeath(in ntt, ref dtc);
            else if (ntt.Type == EntityType.Item)
                ItemDeath(in ntt);
            // else if (ntt.Type == EntityType.Trap)
            //     TrapDeath(in ntt, ref dtc);
        }

        public static void EntityDeath(in NTT ntt, ref DeathTagComponent dtc)
        {
            if (dtc.Tick == NttWorld.Tick)
            {
                ref readonly var pos = ref ntt.Get<PositionComponent>();

                var deathMsg = MsgInteract.Create(in dtc.Killer, in ntt, MsgInteractType.Death, 0);
                ntt.NetSync(ref deathMsg, true);

                if (!ntt.Has<StatusEffectComponent>())
                    ntt.Set(new StatusEffectComponent(in ntt));

                ref var eff = ref ntt.Get<StatusEffectComponent>();
                eff.Effects |= StatusEffect.Dead | StatusEffect.FrozenRemoveName;

                if (ntt.Type == EntityType.Player)
                {
                    ref var bdy = ref ntt.Get<BodyComponent>();
                    var ghostLook = bdy.Look % 10000 is 2001 or 2002 ? MsgSpawn.AddTransform(bdy.Look, 99) : MsgSpawn.AddTransform(bdy.Look, 98);
                    bdy.Look = ghostLook;
                }

                if (ntt.Has<CqActionComponent>())
                {
                    ref readonly var cqc = ref ntt.Get<CqActionComponent>();
                    var action = cqc.cq_Action;
                    for (var i = 0; i < 32; i++)
                    {
                        if (action == 0)
                            break;
                        action = CqActionProcessor.Process(in ntt, in ntt, CqProcessor.GetAction(action));
                    }
                }
                if (ntt.Has<InventoryComponent>())
                {
                    ref var inv = ref ntt.Get<InventoryComponent>();

                    if (inv.Money > 0 && Random.Shared.NextSingle() < 0.25f)
                    {
                        var drop = new RequestDropMoneyComponent(Random.Shared.Next((int)inv.Money));
                        ntt.Set(ref drop);
                    }

                    InventoryHelper.SortById(ntt, ref inv);
                    var itemCount = InventoryHelper.CountItems(ref inv);
                    for (var i = 0; i < itemCount; i++)
                    {
                        if (Random.Shared.NextSingle() >= 0.1f)
                        {
                            ref var itemComp = ref inv.Items.Span[i].Get<ItemComponent>();
                            if (itemComp.Id == 0)
                                continue;
                            var rdi = new RequestDropItemComponent(in inv.Items.Span[i]);
                            ntt.Set(ref rdi);
                        }
                    }
                }

                dtc.Killer.Remove<AttackComponent>();
                ntt.Remove<AttackComponent>();
                ntt.Remove<BrainComponent>();
                ntt.Remove<WalkComponent>();
                ntt.Remove<JumpComponent>();
            }
            else if (dtc.Tick + (NttWorld.TargetTps * 7) == NttWorld.Tick && ntt.Type == EntityType.Monster)
            {
                ref var eff = ref ntt.Get<StatusEffectComponent>();
                eff.Effects |= StatusEffect.Fade;
            }
            else if (dtc.Tick + (NttWorld.TargetTps * 10) == NttWorld.Tick && ntt.Type == EntityType.Monster)
            {
                ref readonly var lgc = ref ntt.Get<LifeGiverComponent>();
                ref var spc = ref lgc.NTT.Get<SpawnerComponent>();
                spc.Count--;
                ntt.Set<DestroyEndOfFrameComponent>();
            }
        }

        public static void ItemDeath(in NTT ntt)
        {
            var despawn = MsgFloorItem.Create(in ntt, MsgFloorItemType.Delete);
            var delete = MsgFloorItem.Create(in ntt, MsgFloorItemType.Delete);
            ntt.NetSync(ref despawn, true);
            ntt.NetSync(ref delete, true);

            ntt.Set<DestroyEndOfFrameComponent>();
        }

        // public static void TrapDeath(in NTT ntt, ref DeathTagComponent dtc)
        // {

        // }
    }
}