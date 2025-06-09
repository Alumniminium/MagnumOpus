using System.Numerics;
using MagnumOpus.Components;
using MagnumOpus.AOGP.Actions;
using MagnumOpus.Squiggly;
using MagnumOpus.Squiggly.Models;
using NttECS.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Helpers;

/// <summary>
/// Factory class for creating game entities including items, money drops, and monsters with proper component setup.
/// Handles entity initialization, component attachment, and spatial hash management for game world objects.
/// </summary>
public static class EntityFactory
{
    /// <summary>
    /// Creates a default item entity with specified properties and optional world placement.
    /// </summary>
    /// <param name="itemId">Item type identifier from item database</param>
    /// <param name="position">World position for item placement (optional)</param>
    /// <param name="map">Map identifier for world placement (optional)</param>
    /// <param name="randomDurability">Whether to randomize item durability</param>
    /// <returns>Created item entity or default if item type not found</returns>
    public static NTT MakeDefaultItem(int itemId, Vector2 position = default, int map = 0, bool randomDurability = false)
    {
        if (Collections.ItemType.TryGetValue(itemId, out var itemType) == false)
            return default;

        var durability = randomDurability ? (ushort)(1 + (Random.Shared.NextSingle() * itemType.AmountLimit)) : itemType.AmountLimit;

        ref var ntt = ref NttWorld.CreateEntity(IdGenerator.GetItemId());
        var itemInfo = new ItemComponent(itemId, durability, itemType.AmountLimit, 0, 0, 0, 0, 0, 0, 0, 0);

        if (position != Vector2.Zero && map != 0)
        {
            var pos = new PositionComponent(position, map);
            var shr = new SpatialHashUpdateComponent(position, Vector2.Zero, map, map, SpacialHashUpdatType.Add);

            ntt.Set(ref pos);
            ntt.Set(ref shr);
            ntt.Set<ViewportUpdateTagComponent>();
        }

        ntt.Set(ref itemInfo);

        return ntt;
    }

    /// <summary>
    /// Creates a money drop entity on the ground with limited lifetime and pickup mechanics.
    /// </summary>
    /// <param name="amount">Amount of money to drop</param>
    /// <param name="pos">Position component defining drop location</param>
    /// <returns>Created money drop entity or default if creation failed</returns>
    public static NTT MakeMoneyDrop(int amount, ref PositionComponent pos)
    {
        var itemId = ItemHelper.GetItemIdFromMoney(amount);
        var ntt = MakeDefaultItem(itemId, pos.Position, pos.Map);

        if (ntt == default)
            return default;

        var ltc = new LifeTimeComponent(TimeSpan.FromSeconds(30));
        var vwp = new ViewportComponent(18f);
        var shr = new SpatialHashUpdateComponent(pos.Position, Vector2.Zero, pos.Map, pos.Map, SpacialHashUpdatType.Add);
        var moneyInfo = new MoneyRewardComponent(amount);

        ntt.Set(ref vwp);
        ntt.Set(ref ltc);
        ntt.Set(ref moneyInfo);
        ntt.Set(ref shr);
        ntt.Set<ViewportUpdateTagComponent>();

        PrometheusPush.MoneyDropCount.Inc();
        PrometheusPush.MoneyDropTotal.Inc(amount);
        PrometheusPush.ServerExpenses.Inc(amount);

        return ntt;
    }

    /// <summary>
    /// Creates a monster entity from database template with AI behavior, inventory, and spawner association.
    /// Configures different AI types based on monster name and spawner properties.
    /// </summary>
    /// <param name="prefab">Monster template from database</param>
    /// <param name="spc">Spawner component for spawn area and counting</param>
    /// <param name="pos">Position component for map placement</param>
    /// <param name="spawner">Spawner entity that created this monster</param>
    /// <returns>Created monster entity with full component setup</returns>
    public static NTT MakeMonster(cq_monstertype prefab, ref SpawnerComponent spc, PositionComponent pos, NTT spawner)
    {
        ref var mob = ref NttWorld.CreateEntity(IdGenerator.GetMonsterId());
        var respawnPos = CoMath.GetRandomPointInRect(in spc.SpawnArea);

        var cqm = new CqMonsterComponent(prefab.id);
        var mpos = new PositionComponent(respawnPos, pos.Map);
        var bdy = new BodyComponent(mob, prefab.lookface);
        var hp = new HealthComponent(mob, prefab.life, prefab.life);
        var vwp = new ViewportComponent(18f);
        var inv = new InventoryComponent(mob, prefab.drop_money, 0);
        var fsp = new LifeGiverComponent(spawner);
        var sfc = new StatusEffectComponent(mob);
        var shr = new SpatialHashUpdateComponent(pos.Position, Vector2.Zero, pos.Map, pos.Map, SpacialHashUpdatType.Add);

        var viewport = vwp.Viewport;
        viewport.X = (int)pos.Position.X;
        viewport.Y = (int)pos.Position.Y;
        vwp.Viewport = viewport;

        if (!prefab.name.Contains("guard", StringComparison.InvariantCultureIgnoreCase))
        {
            if (spc.GeneratorId % 9 == 0)
            {
                var boi = new BoidBehaviorComponent(spc.GeneratorId, mpos.Position);
                mob.Set(ref boi);
            }
            else
            {
                var brn = new BrainComponent(new WalkApproachAction(), new AttackAction());
                mob.Set(ref brn);
            }
        }
        else
        {
            var brn = new BrainComponent();
            mob.Set(ref brn);
            var grd = new GuardPositionComponent(new Vector2(spc.SpawnArea.X, spc.SpawnArea.Y));
            mob.Set(ref grd);
        }

        var items = ItemGenerator.GetDropItemsFor(cqm.CqMonsterId);
        for (var x = 0; x < items.Count; x++)
        {
            var item = items[x];

            if (InventoryHelper.HasItemId(ref inv, item.ID))
                continue;
            if (!InventoryHelper.HasFreeSpace(ref inv))
                break;

            var invItemNtt = MakeDefaultItem(item.ID, default, 0, true);
            if (invItemNtt != default)
                continue;

            InventoryHelper.AddItem(mob, ref inv, invItemNtt);
        }

        if (prefab.action != 0)
        {
            var cq = new CqActionComponent(prefab.action);
            mob.Set(ref cq);
        }

        mob.Set(ref mpos);
        mob.Set(ref bdy);
        mob.Set(ref hp);
        mob.Set(ref vwp);
        mob.Set(ref inv);
        mob.Set(ref cqm);
        mob.Set(ref fsp);
        mob.Set(ref sfc);
        mob.Set(ref shr);
        mob.Set<ViewportUpdateTagComponent>();

        spc.Count++;

        return mob;
    }
    /// <summary>
    /// Creates a monster entity with boid flocking behavior for group movement patterns.
    /// Simplified monster creation for special spawning scenarios.
    /// </summary>
    /// <param name="prefab">Monster template from database</param>
    /// <param name="pos">Position component for map placement</param>
    /// <param name="spawner">Spawner entity that created this monster</param>
    /// <returns>Created monster entity with boid behavior</returns>
    public static NTT MakeMonster(cq_monstertype prefab, PositionComponent pos, NTT spawner)
    {
        ref var mob = ref NttWorld.CreateEntity(IdGenerator.GetMonsterId());
        var respawnPos = CoMath.GetRandomPointInRect(spawner.Get<ViewportComponent>().Viewport);

        var cqm = new CqMonsterComponent(prefab.id);
        var mpos = new PositionComponent(respawnPos, pos.Map);
        var bdy = new BodyComponent(mob, prefab.lookface);
        var hp = new HealthComponent(mob, prefab.life, prefab.life);
        var vwp = new ViewportComponent(18f);
        var inv = new InventoryComponent(mob, prefab.drop_money, 0);
        var fsp = new LifeGiverComponent(spawner);
        var boi = new BoidBehaviorComponent(spawner.Id, mpos.Position);
        var shr = new SpatialHashUpdateComponent(pos.Position, Vector2.Zero, pos.Map, pos.Map, SpacialHashUpdatType.Add);

        mob.Set(ref mpos);
        mob.Set(ref bdy);
        mob.Set(ref hp);
        mob.Set(ref boi);
        mob.Set(ref vwp);
        mob.Set(ref inv);
        mob.Set(ref cqm);
        mob.Set(ref fsp);
        mob.Set(ref shr);
        mob.Set<ViewportUpdateTagComponent>();

        return mob;
    }
}