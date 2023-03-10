using System.Numerics;
using MagnumOpus.Components;
using MagnumOpus.Components.AI;
using MagnumOpus.Components.CQ;
using MagnumOpus.Components.Death;
using MagnumOpus.Components.Entity;
using MagnumOpus.Components.Items;
using MagnumOpus.Components.Location;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;
using MagnumOpus.SpacePartitioning;
using MagnumOpus.Squiggly;

namespace MagnumOpus.Systems
{
    public sealed class MonsterRespawnSystem : NttSystem<SpawnerComponent, PositionComponent>
    {
        public MonsterRespawnSystem() : base("Mob Respawn", threads: 2) { }

        public override void Update(in NTT spawner, ref SpawnerComponent spc, ref PositionComponent pos)
        {
            if (spc.RunTick != NttWorld.Tick)
                return;

            spc.RunTick += NttWorld.TargetTps * spc.TimerSeconds;

            if (spc.Count >= spc.MaxCount)
                return;

            if (!Collections.CqMonsterType.TryGetValue(spc.MonsterId, out var cqMob))
            {
                spawner.Set<DestroyEndOfFrameComponent>();
                return;
            }

            if (!Collections.Maps.TryGetValue(pos.Map, out var cqMap))
            {
                spawner.Set<DestroyEndOfFrameComponent>();
                return;
            }

            if (IsLogging)
                Logger.Debug("{ntt} respawning {num} of {mob} on map {map}", spawner, spc.GenPerTimer, cqMob.name, cqMap);


            for (var i = 0; i < spc.GenPerTimer; i++)
            {
                var prefab = cqMob;
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

                    var invItemNtt = EntityFactory.MakeDefaultItem(item.ID, default, 0, true);
                    if (invItemNtt == null)
                        continue;

                    InventoryHelper.AddItem(in mob, ref inv, invItemNtt.Value);
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
                Collections.SpatialHashs[pos.Map].Add(in mob, ref pos);
                pos.ChangedTick = NttWorld.Tick;

                spc.Count++;

                if (IsLogging)
                {
                    Logger.Debug("{ntt} spawned {mob} at {pos}", mob, cqMob.name, mpos.Position);
                    var msgTalk = MsgText.Create(in spawner, "Respawning " + cqMob.name + " at " + mpos.Position.X + ", " + mpos.Position.Y);
                    spawner.NetSync(ref msgTalk, true);
                }
                if (spc.Count >= spc.MaxCount)
                    break;
            }
        }
    }
}