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
        public MonsterRespawnSystem(bool log = false) : base("Mob Respawn", threads: 1, log) { }

        public override void Update(in NTT _, ref SpawnerComponent spc, ref PositionComponent pos)
        {
            if (spc.RunTick != NttWorld.Tick)
                return;

            spc.RunTick += NttWorld.TargetTps * spc.TimerSeconds;

            if(spc.Count >= spc.MaxCount)
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

            Logger.Debug("{ntt} respawning {num} of {mob} on map {map}", _, spc.GenPerTimer, cqMob.name, cqMap);


            for (int i = 0; i < spc.GenPerTimer; i++)
            {
                var prefab = cqMob;
                ref var obj = ref NttWorld.CreateEntity(EntityType.Monster);
                var respawnPos = CoMath.GetRandomPointInRect(in spc.SpawnArea);

                var cqm = new CqMonsterComponent(obj.Id, prefab.id);
                var mpos = new PositionComponent(obj.Id, respawnPos, pos.Map);
                var bdy = new BodyComponent(obj.Id, prefab.lookface, (Direction)Random.Shared.Next(0, 9));
                var hp = new HealthComponent(obj.Id, prefab.life, prefab.life);
                var ntc = new NameTagComponent(obj.Id, prefab.name.Trim());
                var vwp = new ViewportComponent(obj.Id, 18f);
                var inv = new InventoryComponent(obj.Id, prefab.drop_money, 0);
                var fsp = new FromSpawnerComponent(obj.Id, _.Id);

                var items = ItemGenerator.GetDropItemsFor(cqm.CqMonsterId);
                for (int x = 0; x < items.Count; x++)
                {
                    var item = items[x];

                    if (InventoryHelper.HasItemId(ref inv, item.ID))
                        continue;
                    if (!InventoryHelper.HasFreeSpace(ref inv))
                        continue;

                    var invItemNtt = EntityFactory.MakeDefaultItem(item.ID, default, 0, true);
                    if (invItemNtt == null)
                        continue;

                    InventoryHelper.AddItem(in obj, ref inv, invItemNtt.Value);
                }

                if (prefab.action != 0)
                {
                    var cq = new CqActionComponent(obj.Id, prefab.action);
                    obj.Set(ref cq);
                }

                if (prefab.lookface == 900 || prefab.lookface == 910)
                {
                    pos.Position = new Vector2(pos.Position.X, pos.Position.Y);
                    var grd = new GuardPositionComponent(obj.Id, pos.Position);
                    obj.Set(ref grd);
                }

                vwp.Viewport.X = pos.Position.X;
                vwp.Viewport.Y = pos.Position.Y;
                obj.Set(ref mpos);
                obj.Set(ref bdy);
                obj.Set(ref hp);
                obj.Set(ref ntc);
                obj.Set(ref vwp);
                obj.Set(ref inv);
                obj.Set(ref cqm);
                obj.Set(ref fsp);

                var brn = new BrainComponent(obj.Id);
                obj.Set(ref brn);

                if (!Collections.SpatialHashs.ContainsKey(pos.Map))
                    Collections.SpatialHashs[pos.Map] = new SpatialHash(10);
                Collections.SpatialHashs[pos.Map].Add(in obj);
                pos.ChangedTick = NttWorld.Tick;

                spc.Count++;
                
                var msgTalk = MsgText.Create(in _, "Respawning "+ntc.Name+" at "+pos.Position.X+", "+pos.Position.Y);
                _.NetSync(ref msgTalk,true);
                if(spc.Count >= spc.MaxCount)
                    break;
            }
        }
    }
}