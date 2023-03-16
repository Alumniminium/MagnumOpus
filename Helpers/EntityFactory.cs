using System.Numerics;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.SpacePartitioning;
using MagnumOpus.Squiggly;
using MagnumOpus.Squiggly.Models;

namespace MagnumOpus.Helpers
{
    public static class EntityFactory
    {
        public static NTT MakeDefaultItem(int itemId, Vector2 pos = default, int map = 0, bool randomDurability = false)
        {
            if (Collections.ItemType.TryGetValue(itemId, out var itemType) == false)
                return default;

            var durability = randomDurability ? (ushort)(1 + (Random.Shared.NextSingle() * itemType.AmountLimit)) : itemType.AmountLimit;

            ref var ntt = ref NttWorld.CreateEntity(EntityType.InvItem);
            var itemInfo = new ItemComponent(itemId, durability, itemType.AmountLimit, 0, 0, 0, 0, 0, 0, 0, 0);
            ntt.Set(ref itemInfo);

            if (pos != Vector2.Zero && map != 0)
            {
                var posInfo = new PositionComponent(pos, map);
                ntt.Set(ref posInfo);

                if (!Collections.SpatialHashs.ContainsKey(map))
                    Collections.SpatialHashs[map] = new SpatialHash(10);
                Collections.SpatialHashs[map].Add(ntt, ref posInfo);
                posInfo.ChangedTick = NttWorld.Tick;

            }
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
            ntt.Set(ref vwp);
            ntt.Set(ref ltc);

            var moneyInfo = new MoneyRewardComponent(amount);
            ntt.Set(ref moneyInfo);

            if (!Collections.SpatialHashs.ContainsKey(pos.Map))
                Collections.SpatialHashs[pos.Map] = new SpatialHash(10);
            Collections.SpatialHashs[pos.Map].Add(ntt, ref pos);
            pos.ChangedTick = NttWorld.Tick;

            return ntt;
        }

        public static NTT? MakeMonster(cq_monstertype prefab, ref SpawnerComponent spc, PositionComponent pos, NTT spawner)
        {
            ref var mob = ref NttWorld.CreateEntity(EntityType.Monster);
            var respawnPos = CoMath.GetRandomPointInRect(in spc.SpawnArea);

            var cqm = new CqMonsterComponent(prefab.id);
            var mpos = new PositionComponent(respawnPos, pos.Map);
            var bdy = new BodyComponent(mob, prefab.lookface, (Direction)Random.Shared.Next(0, 9));
            var hp = new HealthComponent(mob, prefab.life, prefab.life);
            var vwp = new ViewportComponent(18f);
            var inv = new InventoryComponent(mob, prefab.drop_money, 0);
            var fsp = new LifeGiverComponent(spawner);

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

            if (prefab.lookface is 900 or 910)
            {
                var grd = new GuardPositionComponent(new Vector2(spc.SpawnArea.X, spc.SpawnArea.Y));
                mob.Set(ref grd);
            }

            vwp.Viewport.X = mpos.Position.X;
            vwp.Viewport.Y = mpos.Position.Y;
            vwp.Dirty = true;
            mob.Set(ref mpos);
            mob.Set(ref bdy);
            mob.Set(ref hp);
            // mob.Set(ref ntc);
            mob.Set(ref vwp);
            mob.Set(ref inv);
            mob.Set(ref cqm);
            mob.Set(ref fsp);

            var brn = new BrainComponent();
            mob.Set(ref brn);

            if (!Collections.SpatialHashs.ContainsKey(pos.Map))
                Collections.SpatialHashs[pos.Map] = new SpatialHash(10);
            Collections.SpatialHashs[pos.Map].Add(mob, ref pos);
            pos.ChangedTick = NttWorld.Tick;

            spc.Count++;

            return mob;
        }
    }
}