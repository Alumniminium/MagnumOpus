using System.Numerics;
using MagnumOpus.AOGP.Actions;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Squiggly;
using MagnumOpus.Squiggly.Models;

namespace MagnumOpus.Helpers
{
    public static class EntityFactory
    {
        public static NTT MakeDefaultItem(int itemId, Vector2 position = default, int map = 0, bool randomDurability = false)
        {
            if (Collections.ItemType.TryGetValue(itemId, out var itemType) == false)
                return default;

            var durability = randomDurability ? (ushort)(1 + (Random.Shared.NextSingle() * itemType.AmountLimit)) : itemType.AmountLimit;

            ref var ntt = ref NttWorld.CreateEntity(EntityType.Item);
            var itemInfo = new ItemComponent(itemId, durability, itemType.AmountLimit, 0, 0, 0, 0, 0, 0, 0, 0);

            if (position != Vector2.Zero && map != 0)
            {
                var pos = new PositionComponent(position, map);
                var shr = new SpatialHashUpdateComponent(position, Vector2.Zero, map, SpacialHashUpdatType.Add);

                ntt.Set(ref pos);
                ntt.Set(ref shr);
                ntt.Set<ViewportUpdateTagComponent>();
            }

            ntt.Set(ref itemInfo);

            return ntt;
        }

        public static NTT MakeMoneyDrop(int amount, ref PositionComponent pos)
        {
            var itemId = ItemHelper.GetItemIdFromMoney(amount);
            var ntt = MakeDefaultItem(itemId, pos.Position, pos.Map);

            if (ntt == default)
                return default;

            var ltc = new LifeTimeComponent(TimeSpan.FromSeconds(30));
            var vwp = new ViewportComponent(18f);
            var shr = new SpatialHashUpdateComponent(pos.Position, Vector2.Zero, pos.Map, SpacialHashUpdatType.Add);
            var moneyInfo = new MoneyRewardComponent(amount);

            ntt.Set(ref vwp);
            ntt.Set(ref ltc);
            ntt.Set(ref moneyInfo);
            ntt.Set(ref shr);
            ntt.Set<ViewportUpdateTagComponent>();

            return ntt;
        }

        public static NTT MakeMonster(cq_monstertype prefab, ref SpawnerComponent spc, PositionComponent pos, NTT spawner)
        {
            ref var mob = ref NttWorld.CreateEntity(EntityType.Monster);
            var respawnPos = CoMath.GetRandomPointInRect(in spc.SpawnArea);

            var cqm = new CqMonsterComponent(prefab.id);
            var mpos = new PositionComponent(respawnPos, pos.Map);
            var bdy = new BodyComponent(mob, prefab.lookface);
            var hp = new HealthComponent(mob, prefab.life, prefab.life);
            var vwp = new ViewportComponent(18f);
            var inv = new InventoryComponent(mob, prefab.drop_money, 0);
            var fsp = new LifeGiverComponent(spawner);
            var sfc = new StatusEffectComponent(mob);
            var shr = new SpatialHashUpdateComponent(pos.Position, Vector2.Zero, pos.Map, SpacialHashUpdatType.Add);

            vwp.Viewport.X = (int)pos.Position.X;
            vwp.Viewport.Y = (int)pos.Position.Y;

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
        public static NTT MakeMonster(cq_monstertype prefab, PositionComponent pos, NTT spawner)
        {
            ref var mob = ref NttWorld.CreateEntity(EntityType.Monster);
            var respawnPos = CoMath.GetRandomPointInRect(spawner.Get<ViewportComponent>().Viewport);

            var cqm = new CqMonsterComponent(prefab.id);
            var mpos = new PositionComponent(respawnPos, pos.Map);
            var bdy = new BodyComponent(mob, prefab.lookface);
            var hp = new HealthComponent(mob, prefab.life, prefab.life);
            var vwp = new ViewportComponent(18f);
            var inv = new InventoryComponent(mob, prefab.drop_money, 0);
            var fsp = new LifeGiverComponent(spawner);
            var boi = new BoidBehaviorComponent(spawner.Id, mpos.Position);
            var shr = new SpatialHashUpdateComponent(pos.Position, Vector2.Zero, pos.Map, SpacialHashUpdatType.Add);

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
}