using System.Numerics;
using MagnumOpus.Components;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;
using NttECS.ECS;

namespace MagnumOpus.Systems;

public sealed class DeathSystem : NttSystem<DeathTagComponent>
{
    public DeathSystem() : base("Death", threads: 1, log: false) { }

    // Handles entity death processing with different behaviors for each entity type. Living entities
    // (players, NPCs, monsters) go through a multi-stage death process: immediate death effects,
    // item/money drops, visual changes (ghost form for players), fade effects after 7 seconds,
    // and final cleanup after 10 seconds. Items are immediately despawned and destroyed.
    public override void Update(in NTT ntt, ref DeathTagComponent dtc)
    {
        // Route to appropriate death handler based on entity type
        if (ntt.IsPlayer() || ntt.IsNpc() || ntt.IsMonster(guardsAreMonsters: true))
        {
            // === LIVING ENTITY DEATH PROCESSING ===

            if (dtc.Tick == NttWorld.Tick)
            {
                // Initial death frame - apply all immediate death effects
                ref readonly var pos = ref ntt.Get<PositionComponent>();

                // Broadcast death animation to nearby players
                var deathMessage = MsgInteract.Create(in dtc.Killer, in ntt, MsgInteractType.Death, 0);
                ntt.NetSync(ref deathMessage, true);

                // Apply death status effects
                ref var statusEffects = ref ntt.Get<StatusEffectComponent>();
                statusEffects.Effects |= StatusEffect.Dead | StatusEffect.FrozenRemoveName;

                // Transform players into ghost form
                if (ntt.IsPlayer())
                {
                    ref var body = ref ntt.Get<BodyComponent>();
                    var ghostLook = body.Look % 10000 is 2001 or 2002
                        ? MsgSpawn.AddTransform(body.Look, 99)
                        : MsgSpawn.AddTransform(body.Look, 98);
                    body.Look = ghostLook;
                }

                // Execute death-related CQ actions (scripts)
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

                // Handle item and money drops for entities with inventory
                if (ntt.Has<InventoryComponent>())
                {
                    ref var inventory = ref ntt.Get<InventoryComponent>();

                    // 25% chance to drop some money
                    if (inventory.Money > 0 && Random.Shared.NextSingle() < 0.25f)
                    {
                        var moneyDrop = new RequestDropMoneyComponent(Random.Shared.Next(1, (int)inventory.Money));
                        ntt.Set(ref moneyDrop);
                    }

                    // Drop items with 10% chance per item
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

                // Remove movement and combat components to stop all actions
                dtc.Killer.Remove<AttackComponent>();
                ntt.Remove<AttackComponent>();
                ntt.Remove<BrainComponent>();
                ntt.Remove<WalkComponent>();
                ntt.Remove<JumpComponent>();
                ntt.Remove<BoidBehaviorComponent>();
            }
            else if (dtc.Tick + (NttWorld.TargetTps * 7) == NttWorld.Tick && ntt.IsMonster(guardsAreMonsters: true))
            {
                // After 7 seconds - start fade effect for monsters
                ref var statusEffects = ref ntt.Get<StatusEffectComponent>();
                statusEffects.Effects |= StatusEffect.Fade;
            }
            else if (dtc.Tick + (NttWorld.TargetTps * 10) <= NttWorld.Tick && ntt.IsMonster(guardsAreMonsters: true))
            {
                // After 10 seconds - final cleanup for monsters

                // Decrement spawner count if entity was spawned
                if (ntt.Has<LifeGiverComponent>())
                {
                    ref readonly var lifeGiver = ref ntt.Get<LifeGiverComponent>();
                    ref var spawner = ref lifeGiver.NTT.Get<SpawnerComponent>();
                    spawner.Count--;
                }

                // Remove from spatial hash and mark for destruction
                ref readonly var position = ref ntt.Get<PositionComponent>();
                var spatialRemoval = new SpatialHashUpdateComponent(position.Position, Vector2.Zero,
                    position.Map, position.Map, SpacialHashUpdatType.Remove);
                ntt.Set(ref spatialRemoval);

                ntt.Set<DestroyEndOfFrameComponent>();
            }
            return;
        }

        if (ntt.IsItem())
        {
            // === ITEM DEATH PROCESSING ===
            // Items are immediately despawned and destroyed

            var despawn = MsgFloorItem.Create(in ntt, MsgFloorItemType.Delete);
            var delete = MsgFloorItem.Create(in ntt, MsgFloorItemType.Delete);
            ntt.NetSync(ref despawn, true);
            ntt.NetSync(ref delete, true);

            ntt.Set<DestroyEndOfFrameComponent>();
        }
    }
}