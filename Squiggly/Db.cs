using System.Diagnostics;
using System.Numerics;
using System.Text;
using Co2Core.IO;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
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
                    amount = (ushort)(amount * 4);

                for (var i = 0; i < amount; i++)
                {
                    var prefab = Collections.BaseMonsters[spawn.Value.MobId];
                    ref var obj = ref PixelWorld.CreateEntity(EntityType.Monster);

                    var spw = new SpawnComponent(obj.Id, spawn.Key);
                    var pos = new PositionComponent(obj.Id, new Vector2(spawn.Value.Xstart, spawn.Value.Ystart), spawn.Value.MapId);
                    var bdy = new BodyComponent(obj.Id, (uint)prefab.Look);
                    var dir = new DirectionComponent(obj.Id, (Direction)Random.Shared.Next(0, 9));
                    var hp = new HealthComponent(obj.Id, prefab.Health, prefab.MaxHealth);
                    var ntc = new NameTagComponent(obj.Id, prefab.Name);
                    var vwp = new ViewportComponent(obj.Id, 40f);
                    var inv = new InventoryComponent(obj.Id, 0, 0);

                    if (prefab.CQAction != 0)
                    {
                        var cq = new CqActionComponent(obj.Id, prefab.CQAction);
                        obj.Set(ref cq);
                    }

                    pos.Position.X = (ushort)Random.Shared.Next(spawn.Value.Xstart - 10, spawn.Value.Xstart + spawn.Value.Xend + 10);
                    pos.Position.Y = (ushort)Random.Shared.Next(spawn.Value.Ystart - 10, spawn.Value.Ystart + spawn.Value.Yend + 10);

                    if (monster.Look == 900 || monster.Look == 910)
                    {
                        pos.Position = new Vector2(spawn.Value.Xstart, spawn.Value.Ystart);
                        var grd = new GuardPositionComponent(obj.Id, pos.Position);
                        obj.Set(ref grd);
                    }

                    obj.Set(ref spw);
                    obj.Set(ref pos);
                    obj.Set(ref bdy);
                    obj.Set(ref dir);
                    obj.Set(ref hp);
                    obj.Set(ref ntc);
                    obj.Set(ref vwp);
                    // obj.Set(ref drop);

                    var brn = new BrainComponent(obj.Id);
                    obj.Set(ref brn);

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
            FConsole.WriteLine($"[MobGenerator] Spawnd {Collections.Monsters.Count}\t Monsters in {sw.Elapsed.TotalMilliseconds}ms");
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
                    if (!Game.Grids.TryGetValue((ushort)cqmap.id, out var grid))
                    {
                        grid = new Grid(cqmap.Width, cqmap.Height, 10, 10);
                        Game.Grids.Add((ushort)cqmap.id, grid);
                    }
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
            FConsole.WriteLine($"[SquigglyLite] Loaded {Collections.Maps.Count}\t Maps in {sw.Elapsed.TotalMilliseconds}ms");
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
                    var vwp = new ViewportComponent(ntt.Id, 40);
                    ntt.Set(ref pos);
                    ntt.Set(ref bdy);
                    ntt.Set(ref npcc);
                    ntt.Set(ref vwp);

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
            FConsole.WriteLine($"[SquigglyLite] Loaded \t Npcs in {sw.Elapsed.TotalMilliseconds}ms");
        }
        public static unsafe void LoadMobs()
        {
            var sw = Stopwatch.StartNew();
            using (var db = new SquigglyContext())
            {
                foreach (var cqMob in db.cq_monstertype.AsQueryable().OrderBy(x => x.level))
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

                    var drop = mob.Drops;
                    var possibleTypes = new List<(int, ushort[])>();
                    if (drop.Armet != 99)
                        possibleTypes.Add((drop.Armet, ItemGenerator.ArmetType));
                    if (drop.Armor != 99)
                        possibleTypes.Add((drop.Armor, ItemGenerator.ArmorType));
                    if (drop.Necklace != 99)
                        possibleTypes.Add((drop.Necklace, ItemGenerator.NecklaceType));
                    if (drop.Ring != 99)
                        possibleTypes.Add((drop.Ring, ItemGenerator.RingType));
                    if (drop.Weapon != 99)
                    {
                        possibleTypes.Add((drop.Weapon, ItemGenerator.OneHanderType));
                        possibleTypes.Add((drop.Weapon, ItemGenerator.TwoHanderType));
                    }

                    if (Collections.ItemType.TryGetValue(drop.Hp, out var hp))
                        mob.Drops.Items.Add(hp);

                    if (Collections.ItemType.TryGetValue(drop.Mp, out var mp))
                        mob.Drops.Items.Add(mp);

                    if (possibleTypes.Count > 0)
                    {
                        foreach (var kvp in possibleTypes)
                        {
                            var level = kvp.Item1;
                            foreach (var t in kvp.Item2)
                            {
                                for (int r = 0; r < 10; r++)
                                {

                                    var item = new ItemComponent();
                                    item.Id = t * 1000 + level * 10;

                                    if (r > 0)
                                    {
                                        if (ItemGenerator.ArmetType.Contains(t))
                                            item.Id += 300;
                                        if (ItemGenerator.ArmorType.Contains(t))
                                            item.Id += 100;
                                        if (ItemGenerator.NecklaceType.Contains(t))
                                            item.Id += 10;
                                        if (ItemGenerator.RingType.Contains(t))
                                            item.Id -= 90;
                                    }
                                    if (r > 1)
                                    {
                                        if (ItemGenerator.ArmorType.Contains(t))
                                            item.Id += 100;
                                    }
                                    if (r > 2)
                                    {
                                        if (ItemGenerator.ArmorType.Contains(t))
                                            item.Id += 100;
                                    }
                                    if (r > 3)
                                    {
                                        if (ItemGenerator.ArmorType.Contains(t))
                                            item.Id += 100;
                                    }
                                    if (r > 4)
                                    {
                                        if (ItemGenerator.ArmorType.Contains(t))
                                            item.Id += 100;
                                    }
                                    if (r > 5)
                                    {
                                        if (ItemGenerator.ArmorType.Contains(t))
                                            item.Id += 100;
                                    }
                                    if (r > 6)
                                    {
                                        if (ItemGenerator.ArmorType.Contains(t))
                                            item.Id += 100;
                                    }
                                    if (r > 7)
                                    {
                                        if (ItemGenerator.ArmorType.Contains(t))
                                            item.Id += 100;
                                    }
                                    if (r > 8)
                                    {
                                        if (ItemGenerator.ArmorType.Contains(t))
                                            item.Id += 100;
                                    }

                                    var entry = default(ItemType.Entry);
                                    for (int i = -2; i < 3; i++)
                                    {
                                        item.Id = t * 1000 + level * 10;
                                        item.Id += i * 100;

                                        for (int ii = 0; ii < 5; ii++)
                                        {
                                            if (Collections.ItemType.TryGetValue(item.Id, out entry))
                                                break;
                                            item.Id += ii;
                                        }
                                        if (Collections.ItemType.TryGetValue(item.Id, out entry))
                                            break;
                                    }

                                    if (entry.ID == 0 || entry.RequiredLevel + 10 < mob.Level || entry.RequiredLevel - 10 > mob.Level)
                                    {
                                        // FConsole.WriteLine($"[{nameof(ItemGenerator)}] {mob.Name} (Level: {mob.Level}) Generated invalid {item.Id} - not found");
                                        continue;
                                    }
                                    // FConsole.WriteLine($"[{nameof(ItemGenerator)}] {mob.Name} (Level: {mob.Level}) drops {Encoding.UTF8.GetString(entry.Name, ItemType.MAX_NAMESIZE).Trim('\0')} (Level: {entry.RequiredLevel}) ({item.Id})");
                                    var exists = false;
                                    foreach (var d in mob.Drops.Items)
                                    {
                                        // 1st digit from the right is the quality
                                        var id = d.ID / 10 * 10;
                                        var newId = item.Id / 10 * 10;

                                        id += 3;
                                        newId += 3;

                                        // 3rd digit from the left is the color
                                        var color = d.ID % 1000 / 100 * 100;
                                        var newColor = item.Id % 1000 / 100 * 100;
                                        id -= color;
                                        newId -= newColor;

                                        if (id == newId)
                                        {
                                            exists = true;
                                            break;
                                        }
                                    }
                                    if (!exists)
                                        mob.Drops.Items.Add(entry);
                                }
                            }
                        }
                    }

                    Collections.BaseMonsters.Add(mob.Id, mob);
                    if (mob.Drops.Items.Count > 0)
                        FConsole.WriteLine($"[{nameof(ItemGenerator)}] {mob.Drops.Items.Count} drops for {mob.Name} (Level: {mob.Level})");
                }
            }
            sw.Stop();
            FConsole.WriteLine($"[SquigglyLite] Loaded {Collections.BaseMonsters.Count}\t BaseMonsters in {sw.Elapsed.TotalMilliseconds}ms");
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
            FConsole.WriteLine($"[SquigglyLite] Loaded {Collections.Spawns.Count}\t Spawns in {sw.Elapsed.TotalMilliseconds}ms");
        }

        public static void LoadLevelExp()
        {
            var sw = Stopwatch.StartNew();
            var exp = new Co2Core.IO.LevelExp();
            exp.LoadFromDat("CLIENT_FILES/LevelExp.dat");
            exp.
            sw.Stop();
            FConsole.WriteLine($"[SquigglyLite] Loaded {Collections.LevelExps.Count}\t LevelExps in {sw.Elapsed.TotalMilliseconds}ms");
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
                foreach (var dmapPortal in db.Dmap_Portals.AsQueryable())
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
                foreach (var passway in db.cq_passway)
                {
                    Collections.CqPassway.Add(passway);
                }
            }
            sw.Stop();
            FConsole.WriteLine($"[SquigglyLite] Loaded {Collections.CqPortal.Count}\t Portals in {sw.Elapsed.TotalMilliseconds}ms");
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
            FConsole.WriteLine($"[SquigglyLite] Loaded {Collections.ItemBonus.Count}\t Item Bonus Stats in {sw.Elapsed.TotalMilliseconds}ms");
        }

        public static void LoadShopDat(string path)
        {
            var ini = new IniFile(path);
            ini.Load();
            var dict = ini.GetDictionary();

            foreach (var headerKvp in dict)
            {
                if (headerKvp.Key.ToLowerInvariant() == "[header]")
                    continue;

                var shopId = int.Parse(headerKvp.Value["ID"]);
                var name = headerKvp.Value["Name"];
                var type = int.Parse(headerKvp.Value["Type"]);
                var moneyType = int.Parse(headerKvp.Value["MoneyType"]);
                var count = int.Parse(headerKvp.Value["ItemAmount"]);

                var items = new List<int>();
                for (int i = 0; i < count; i++)
                {
                    var item = int.Parse(headerKvp.Value[$"Item{i}"]);
                    items.Add(item);
                }

                var shop = new ShopDatEntry(shopId, name, type, moneyType, items);
                Collections.Shops.Add(shopId, shop);
            }
            FConsole.WriteLine($"{Collections.Shops.Count} shops with {Collections.Shops.Sum(x => x.Value.Items.Count)} total items loaded.");
        }
    }
}