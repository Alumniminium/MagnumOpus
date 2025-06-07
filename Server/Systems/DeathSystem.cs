using System.Numerics;
using MagnumOpus.Components;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;
using NttECS.ECS;

namespace MagnumOpus.Systems
{
    /// <summary>
    /// Handles entity death processing including visual effects, item drops, and cleanup.
    /// Manages different death behaviors for players, monsters, NPCs, and items.
    /// </summary>
    public sealed class DeathSystem : NttSystem<DeathTagComponent>
    {
        /// <summary>
        /// Initializes the DeathSystem with full multi-threaded processing capabilities.
        /// </summary>
        public DeathSystem() : base("Death", threads: 1, log: false) { }

        /// <summary>
        /// Processes death for different entity types, routing to appropriate death handlers.
        /// </summary>
        /// <param name="ntt">The entity that has died</param>
        /// <param name="dtc">Death tag component containing death timing and killer information</param>
        public override void Update(in NTT ntt, ref DeathTagComponent dtc)
        {
            if (ntt.IsPlayer() || ntt.IsNpc() || ntt.IsMonster())
                EntityDeath(in ntt, ref dtc);
            else if (ntt.IsItem())
                ItemDeath(in ntt);
        }

        /// <summary>
        /// Handles death processing for living entities including visual effects, item drops, and eventual cleanup.
        /// Manages the full death lifecycle from initial death to final despawn for monsters.
        /// </summary>
        /// <param name="ntt">The dying entity</param>
        /// <param name="dtc">Death tag component with timing and killer data</param>
        public static void EntityDeath(in NTT ntt, ref DeathTagComponent dtc)
        {
            if (dtc.Tick == NttWorld.Tick)
            {
                ref readonly var pos = ref ntt.Get<PositionComponent>();

                var deathMessage = MsgInteract.Create(in dtc.Killer, in ntt, MsgInteractType.Death, 0);
                ntt.NetSync(ref deathMessage, true);

                ref var statusEffects = ref ntt.Get<StatusEffectComponent>();
                statusEffects.Effects |= StatusEffect.Dead | StatusEffect.FrozenRemoveName;

                if (ntt.IsPlayer())
                {
                    ref var body = ref ntt.Get<BodyComponent>();
                    var ghostLook = body.Look % 10000 is 2001 or 2002 ? MsgSpawn.AddTransform(body.Look, 99) : MsgSpawn.AddTransform(body.Look, 98);
                    body.Look = ghostLook;
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
                    ref var inventory = ref ntt.Get<InventoryComponent>();

                    if (inventory.Money > 0 && Random.Shared.NextSingle() < 0.25f)
                    {
                        var moneyDrop = new RequestDropMoneyComponent(Random.Shared.Next(1, (int)inventory.Money));
                        ntt.Set(ref moneyDrop);
                    }

                    InventoryHelper.SortById(ntt, ref inventory);
                    var itemCount = InventoryHelper.CountItems(ref inventory);
                    for (var i = 0; i < itemCount; i++)
                    {
                        if (Random.Shared.NextSingle() >= 0.1f)
                        {
                            ref var itemComponent = ref inventory.Items.Span[i].Get<ItemComponent>();
                            if (itemComponent.Id == 0)
                                continue;
                            var itemDrop = new RequestDropItemComponent(in inventory.Items.Span[i]);
                            ntt.Set(ref itemDrop);
                        }
                    }
                }

                dtc.Killer.Remove<AttackComponent>();
                ntt.Remove<AttackComponent>();
                ntt.Remove<BrainComponent>();
                ntt.Remove<WalkComponent>();
                ntt.Remove<JumpComponent>();
                ntt.Remove<BoidBehaviorComponent>();
            }
            else if (dtc.Tick + (NttWorld.TargetTps * 7) == NttWorld.Tick && ntt.IsMonster())
            {
                ref var statusEffects = ref ntt.Get<StatusEffectComponent>();
                statusEffects.Effects |= StatusEffect.Fade;
            }
            else if (dtc.Tick + (NttWorld.TargetTps * 10) <= NttWorld.Tick && ntt.IsMonster())
            {
                if (ntt.Has<LifeGiverComponent>())
                {
                    ref readonly var lifeGiver = ref ntt.Get<LifeGiverComponent>();
                    ref var spawner = ref lifeGiver.NTT.Get<SpawnerComponent>();
                    spawner.Count--;
                }

                ref readonly var position = ref ntt.Get<PositionComponent>();
                var spatialRemoval = new SpatialHashUpdateComponent(position.Position, Vector2.Zero, position.Map, position.Map, SpacialHashUpdatType.Remove);
                ntt.Set(ref spatialRemoval);

                ntt.Set<DestroyEndOfFrameComponent>();
            }
        }

        /// <summary>
        /// Handles death processing for item entities, sending despawn packets and marking for destruction.
        /// </summary>
        /// <param name="ntt">The item entity to process death for</param>
        public static void ItemDeath(in NTT ntt)
        {
            var despawn = MsgFloorItem.Create(in ntt, MsgFloorItemType.Delete);
            var delete = MsgFloorItem.Create(in ntt, MsgFloorItemType.Delete);
            ntt.NetSync(ref despawn, true);
            ntt.NetSync(ref delete, true);

            ntt.Set<DestroyEndOfFrameComponent>();
        }
    }
}