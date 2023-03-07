using System.Numerics;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;
using MagnumOpus.SpacePartitioning;
using MagnumOpus.Squiggly;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class MonsterRespawnSystem : NttSystem<SpawnerComponent, PositionComponent>
    {
        public MonsterRespawnSystem() : base("Mob Respawn", threads: 2) { }

        public override void Update(in NTT _, ref SpawnerComponent spc, ref PositionComponent pos)
        {
            if (spc.RunTick != NttWorld.Tick)
                return;

            spc.RunTick += NttWorld.TargetTps * spc.TimerSeconds;

            if (spc.Count >= spc.MaxCount)
                return;

            if (!Collections.CqMonsterType.TryGetValue(spc.MonsterId, out var cqMob))
            {
                _.Set(new DestroyEndOfFrameComponent(_.Id));
                return;
            }

            if (!Collections.Maps.TryGetValue(pos.Map, out var cqMap))
            {
                _.Set(new DestroyEndOfFrameComponent(_.Id));
                return;
            }

            if (Trace)
                Logger.Debug("{ntt} respawning {num} of {mob} on map {map}", _, spc.GenPerTimer, cqMob.name, cqMap);


            for (int i = 0; i < spc.GenPerTimer; i++)
            {
                var prefab = cqMob;
                ref var mob = ref NttWorld.CreateEntity(EntityType.Monster);
                var respawnPos = CoMath.GetRandomPointInRect(in spc.SpawnArea);

                var cqm = new CqMonsterComponent(mob.Id, prefab.id);
                var mpos = new PositionComponent(mob.Id, respawnPos, pos.Map);
                var bdy = new BodyComponent(mob.Id, prefab.lookface, (Direction)Random.Shared.Next(0, 9));
                var hp = new HealthComponent(mob.Id, prefab.life, prefab.life);
                var ntc = new NameTagComponent(mob.Id, prefab.name.Trim());
                var vwp = new ViewportComponent(mob.Id, 18f);
                var inv = new InventoryComponent(mob.Id, prefab.drop_money, 0);
                var fsp = new FromSpawnerComponent(mob.Id, _.Id);

                var items = ItemGenerator.GetDropItemsFor(cqm.CqMonsterId);
                for (int x = 0; x < items.Count; x++)
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
                    var cq = new CqActionComponent(mob.Id, prefab.action);
                    mob.Set(ref cq);
                }

                if (prefab.lookface == 900 || prefab.lookface == 910)
                {
                    var grd = new GuardPositionComponent(mob.Id, new Vector2(spc.SpawnArea.X, spc.SpawnArea.Y));
                    mob.Set(ref grd);
                }

                vwp.Viewport.X = mpos.Position.X;
                vwp.Viewport.Y = mpos.Position.Y;
                mob.Set(ref mpos);
                mob.Set(ref bdy);
                mob.Set(ref hp);
                mob.Set(ref ntc);
                mob.Set(ref vwp);
                mob.Set(ref inv);
                mob.Set(ref cqm);
                mob.Set(ref fsp);

                var brn = new BrainComponent(mob.Id);
                mob.Set(ref brn);

                if (!Collections.SpatialHashs.ContainsKey(pos.Map))
                    Collections.SpatialHashs[pos.Map] = new SpatialHash(10);
                Collections.SpatialHashs[pos.Map].Add(in mob);
                pos.ChangedTick = NttWorld.Tick;

                spc.Count++;

                if (Trace)
                {
                    Logger.Debug("{ntt} spawned {mob} at {pos}", mob, cqMob.name, mpos.Position);
                    var msgTalk = MsgText.Create(in _, "Respawning " + ntc.Name + " at " + mpos.Position.X + ", " + mpos.Position.Y);
                    _.NetSync(ref msgTalk, true);
                }
                if (spc.Count >= spc.MaxCount)
                    break;
            }
        }
    }
}