using System.Diagnostics;
using System.Numerics;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Squiggly.Models;
using SpacePartitioning;

namespace MagnumOpus.Squiggly
{
    public static partial class SquigglyDb
    {
        public static void Spawn()
        {
            var sw = Stopwatch.StartNew();
            foreach (var spawn in Collections.Spawns)
            {
                var amount = spawn.Value.Amount;

                if (!Collections.BaseMonsters.TryGetValue(spawn.Value.MobId, out var monster))
                    return;

                if (monster.Look != 900 && monster.Look != 910)
                    amount = (ushort)(amount * 9);

                for (var i = 0; i < amount; i++)
                {
                    var prefab = Collections.BaseMonsters[spawn.Value.MobId];
                    ref var obj = ref PixelWorld.CreateEntity(EntityType.Monster);

                    var spw = new SpawnComponent(obj.Id, spawn.Key);
                    var pos = new PositionComponent(obj.Id, new Vector2(spawn.Value.Xstart, spawn.Value.Ystart), spawn.Value.MapId);
                    var bdy = new BodyComponent(obj.Id, (uint)prefab.Look);
                    var dir = new DirectionComponent(obj.Id, (Direction)Random.Shared.Next(0, 9));
                    var hp = new HealthComponent(obj.Id, prefab.Health, prefab.MaxHealth);
                    var sync = new NetSyncComponent(obj.Id, SyncThings.All);
                    var ntc = new NameTagComponent(obj.Id, prefab.Name);
                    var vwp = new ViewportComponent(obj.Id, 8f);

                    pos.Position.X = (ushort)Random.Shared.Next(spawn.Value.Xstart - 10, spawn.Value.Xstart + spawn.Value.Xend + 10);
                    pos.Position.Y = (ushort)Random.Shared.Next(spawn.Value.Ystart - 10, spawn.Value.Ystart + spawn.Value.Yend + 10);
                    obj.Add(ref spw);
                    obj.Add(ref pos);
                    obj.Add(ref bdy);
                    obj.Add(ref dir);
                    obj.Add(ref hp);
                    obj.Add(ref sync);
                    obj.Add(ref ntc);
                    obj.Add(ref vwp);

                    if (prefab.Look != 900 && prefab.Look != 910 )
                    {
                        var brn = new BrainComponent(obj.Id);
                        obj.Add(ref brn);
                    }

                    if (!Game.Grids.ContainsKey(pos.Map))
                    {
                        if (!Collections.Maps.TryGetValue(pos.Map, out var map))
                            continue;

                        Game.Grids[pos.Map] = new Grid(map.Width, map.Height, 10, 10);
                    }
                    Game.Grids[pos.Map].Add(in obj, ref pos);
                }
            }
            sw.Stop();
            Debug.WriteLine($"[MobGenerator] Spawnd {Collections.Monsters.Count}\t Monsters in {sw.Elapsed.TotalMilliseconds}ms");
        }

        public static void LoadMaps()
        {
            var sw = Stopwatch.StartNew();
            using (var db = new SquigglyContext())
            {
                foreach (var cqmap in db.cq_map.AsQueryable())
                {
                    var map = new CqMap
                    (
                        (ushort)cqmap.id,
                        (ushort)cqmap.mapdoc,
                        (MapFlags)cqmap.type,
                        cqmap.name.Trim(),
                        new Tuple<ushort, ushort, ushort>((ushort)cqmap.portal0_x, (ushort)cqmap.portal0_y, (ushort)cqmap.reborn_map),
                        cqmap.Width,
                        cqmap.Height, new Dictionary<ushort, CqPortal>()
                    );
                    Collections.Maps.Add((ushort)cqmap.id, map);
                }
                foreach (var dportal in db.Dmap_Portals)
                {
                    if (Collections.Maps.TryGetValue(dportal.MapId, out var map))
                    {
                        var passageInfo = new CqPortal(dportal.MapId, dportal.X, dportal.Y, dportal.PortalId, dportal.Id);
                        map.Portals.Add(dportal.PortalId, passageInfo);
                    }
                }
            }

            sw.Stop();
            Debug.WriteLine($"[SquigglyLite] Loaded {Collections.Maps.Count}\t Maps in {sw.Elapsed.TotalMilliseconds}ms");
        }
        public static void LoadNpcs()
        {
            var sw = Stopwatch.StartNew();
            using (var db = new SquigglyContext())
            {
                foreach (var cqNpc in db.cq_npc.AsQueryable())
                {
                    var npc = new CqNpc((int)cqNpc.id, new Vector2(cqNpc.cellx, cqNpc.celly),
                        cqNpc.mapid, cqNpc.sort, cqNpc.@base, cqNpc.type, cqNpc.lookface, cqNpc.name.Trim(), cqNpc.task0, cqNpc.task1,
                        cqNpc.task2, cqNpc.task3, cqNpc.task4, cqNpc.task5, cqNpc.task6, cqNpc.task7);

                    var ntt = PixelWorld.CreateEntityWithNetId(EntityType.Npc, (int)cqNpc.id);
                    var pos = new PositionComponent(ntt.Id, new Vector2(cqNpc.cellx, cqNpc.celly), cqNpc.mapid);
                    var bdy = new BodyComponent(ntt.Id, cqNpc.lookface);
                    var npcc = new NpcComponent(ntt.Id, npc.Base, npc.Type, npc.Sort);
                    var sync = new NetSyncComponent(ntt.Id, SyncThings.All);
                    ntt.Add(ref pos);
                    ntt.Add(ref bdy);
                    ntt.Add(ref npcc);
                    ntt.Add(ref sync);
                    if (!Game.Grids.ContainsKey(pos.Map))
                    {
                        if (!Collections.Maps.TryGetValue(pos.Map, out var map))
                            continue;

                        Game.Grids[pos.Map] = new Grid(map.Width, map.Height, 10, 10);
                    }
                    Game.Grids[pos.Map].Add(in ntt, ref pos);
                }
            }
            sw.Stop();
            Debug.WriteLine($"[SquigglyLite] Loaded \t Npcs in {sw.Elapsed.TotalMilliseconds}ms");
        }
        public static void LoadMobs()
        {
            var sw = Stopwatch.StartNew();
            using (var db = new SquigglyContext())
            {
                foreach (var cqMob in db.cq_monstertype.AsQueryable())
                {
                    var mob = new CqMonster(
                        cqMob.id,
                        cqMob.name.Trim(),
                        cqMob.lookface,
                        cqMob.life,
                        cqMob.life,
                        cqMob.attack_max,
                        cqMob.attack_min,
                        cqMob.defence,
                        cqMob.dexterity,
                        cqMob.dodge,
                        new Drops(cqMob.drop_armet, cqMob.drop_armor, cqMob.drop_weapon, cqMob.drop_hp, cqMob.drop_mp, cqMob.drop_itemtype, cqMob.drop_money, cqMob.drop_necklace, cqMob.drop_ring, cqMob.drop_shield, cqMob.drop_shoes),
                        cqMob.attack_range,
                        cqMob.view_range,
                        cqMob.escape_life,
                        cqMob.attack_speed,
                        cqMob.move_speed,
                        cqMob.run_speed,
                        cqMob.level,
                        cqMob.attack_user,
                        cqMob.action,
                        cqMob.magic_type,
                        cqMob.magic_def,
                        cqMob.magic_hitrate,
                        cqMob.ai_type);

                    if (Collections.BaseMonsters.ContainsKey(mob.Id))
                        continue;

                    Collections.BaseMonsters.Add(mob.Id, mob);
                }
            }
            sw.Stop();
            Debug.WriteLine($"[SquigglyLite] Loaded {Collections.BaseMonsters.Count}\t BaseMonsters in {sw.Elapsed.TotalMilliseconds}ms");
        }
        public static void LoadSpawns()
        {
            var sw = Stopwatch.StartNew();
            using (var db = new SquigglyContext())
            {
                short counter = 0;
                foreach (var cqSpawn in db.cq_generator.AsQueryable())
                {
                    var spawn = new CqSpawnGenerator(cqSpawn.mapid, cqSpawn.npctype, cqSpawn.born_x, cqSpawn.born_y, cqSpawn.timer_begin, cqSpawn.timer_end, cqSpawn.maxnpc, cqSpawn.bound_x, cqSpawn.bound_y, cqSpawn.bound_cx, cqSpawn.bound_cy, cqSpawn.rest_secs, cqSpawn.max_per_gen);
                    if (!Collections.Spawns.ContainsKey(counter))
                        Collections.Spawns.TryAdd(counter, spawn);
                    counter++;
                }
            }
            sw.Stop();
            Debug.WriteLine($"[SquigglyLite] Loaded {Collections.Spawns.Count}\t Spawns in {sw.Elapsed.TotalMilliseconds}ms");
        }

        public static void LoadLevelExp()
        {
            var sw = Stopwatch.StartNew();
            using (var db = new SquigglyContext())
            {
                foreach (var cqLvlExp in db.cq_levexp.AsQueryable())
                {
                    var lvlExp = new CqLevelExp(cqLvlExp.up_lev_time, (byte)cqLvlExp.level, cqLvlExp.exp);
                    Collections.LevelExps.Add((int)cqLvlExp.level, lvlExp);
                }
            }
            sw.Stop();
            Debug.WriteLine($"[SquigglyLite] Loaded {Collections.LevelExps.Count}\t LevelExps in {sw.Elapsed.TotalMilliseconds}ms");
        }

        public static void LoadPortals()
        {
            var sw = Stopwatch.StartNew();
            using (var db = new SquigglyContext())
            {
                foreach (var cqPortal in db.cq_portal.AsQueryable())
                {
                    var portal = new CqPortal(cqPortal.mapid, cqPortal.portal_x, cqPortal.portal_y, cqPortal.id, cqPortal.portal_idx);
                    Collections.CqPortal.Add(portal);
                }
                foreach(var dmapPortal in db.Dmap_Portals.AsQueryable())
                {
                    var portal = new Dmap_Portals()
                    {
                        Id = dmapPortal.Id,
                        MapId = dmapPortal.MapId,
                        PortalId = dmapPortal.PortalId,
                        X = dmapPortal.X,
                        Y = dmapPortal.Y,
                    };
                    Collections.DmapPortals.Add(portal);
                }
                foreach(var passway in db.cq_passway)
                {
                    Collections.CqPassway.Add(passway);
                }
            }
            sw.Stop();
            Debug.WriteLine($"[SquigglyLite] Loaded {Collections.CqPortal.Count}\t Portals in {sw.Elapsed.TotalMilliseconds}ms");
        }

        public static void LoadItemBonus()
        {
            var sw = Stopwatch.StartNew();
            using (var db = new SquigglyContext())
            {
                foreach (var cqItemAdd in db.cq_itemaddition.AsQueryable())
                {
                    var itemBonus = new CqItemBonus(cqItemAdd.id, cqItemAdd.typeid, cqItemAdd.level, cqItemAdd.life, cqItemAdd.attack_max, cqItemAdd.attack_min, cqItemAdd.defense, cqItemAdd.magic_atk, cqItemAdd.magic_def, cqItemAdd.dexterity, cqItemAdd.dodge);
                    Collections.ItemBonus.Add(itemBonus.Id, itemBonus);
                }
            }
            sw.Stop();
            Debug.WriteLine($"[SquigglyLite] Loaded {Collections.ItemBonus.Count}\t Item Bonus Stats in {sw.Elapsed.TotalMilliseconds}ms");
        }
    }
}