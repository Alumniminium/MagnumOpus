using MagnumOpus.Components.AI;
using MagnumOpus.Components.Attack;
using MagnumOpus.Components.CQ;
using MagnumOpus.Components.Death;
using MagnumOpus.Components.Entity;
using MagnumOpus.Components.Items;
using MagnumOpus.Components.Location;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Squiggly;

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
                ItemDeath(in ntt, ref dtc);
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
                    ntt.Set(new StatusEffectComponent(ntt.Id));

                ref var eff = ref ntt.Get<StatusEffectComponent>();
                eff.Effects |= StatusEffect.Dead | StatusEffect.FrozenRemoveName;

                if (ntt.Type == EntityType.Player)
                {
                    ref var bdy = ref ntt.Get<BodyComponent>();
                    var ghostLook = bdy.Look % 10000 is 2001 or 2002 ? MsgSpawn.AddTransform(bdy.Look, 99) : MsgSpawn.AddTransform(bdy.Look, 98);
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

                    InventoryHelper.SortById(in ntt, ref inv);
                    var itemCount = inv.Items.Where(x => x.Id != 0).Count();
                    for (var i = 0; i < itemCount; i++)
                    {
                        if (Random.Shared.NextSingle() >= 0.1f)
                        {
                            inv.Items[i].Set<DestroyEndOfFrameComponent>();
                            continue;
                        }

                        ref var itemComp = ref inv.Items[i].Get<ItemComponent>();
                        if (itemComp.Id == 0)
                            continue;
                        var rdi = new RequestDropItemComponent(in inv.Items[i]);
                        ntt.Set(ref rdi);
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
                ref var pos = ref ntt.Get<PositionComponent>();
                ref readonly var spawn = ref ntt.Get<FromSpawnerComponent>();
                ref readonly var vwp = ref ntt.Get<ViewportComponent>();

                ref readonly var spawner = ref NttWorld.GetEntity(spawn.SpawnerId);
                ref var spc = ref spawner.Get<SpawnerComponent>();
                spc.Count--;

                var despawn = MsgAction.RemoveEntity(ntt.Id);
                ntt.NetSync(ref despawn, true);

                foreach (var b in vwp.EntitiesVisible)
                {
                    if (!b.Value.Has<ViewportComponent>())
                        continue;
                    ref var bVwp = ref b.Value.Get<ViewportComponent>();
                    _ = bVwp.EntitiesVisible.Remove(ntt.Id, out _);
                    _ = bVwp.EntitiesVisibleLast.Remove(ntt.Id, out _);
                }
                vwp.EntitiesVisible.Clear();
                vwp.EntitiesVisibleLast.Clear();

                Collections.SpatialHashs[pos.Map].Remove(in ntt, ref pos);
                ntt.Remove<PositionComponent>();
                ntt.Remove<DeathTagComponent>();
                ntt.Set<DestroyEndOfFrameComponent>();
            }
        }

        public static void ItemDeath(in NTT ntt, ref DeathTagComponent dtc)
        {
            if (dtc.Tick + NttWorld.TargetTps == NttWorld.Tick && ntt.Type == EntityType.Item)
            {
                ref var pos = ref ntt.Get<PositionComponent>();
                var despawn = MsgFloorItem.Create(in ntt, MsgFloorItemType.Delete);
                ntt.NetSync(ref despawn, true);
                var ded = new DestroyEndOfFrameComponent();
                ntt.Set(ref ded);
                if (Collections.SpatialHashs.TryGetValue(pos.Map, out var hash))
                    hash.Remove(in ntt, ref pos);

                var delete = MsgFloorItem.Create(in ntt, MsgFloorItemType.Delete);
                ntt.NetSync(ref delete, true);
            }
        }

        // public static void TrapDeath(in NTT ntt, ref DeathTagComponent dtc)
        // {

        // }
    }
}