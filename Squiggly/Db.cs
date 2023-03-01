using System.Diagnostics;
using System.Numerics;
using Co2Core.Security.Cryptography;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.SpacePartitioning;
using MagnumOpus.Squiggly.Models;

namespace MagnumOpus.Squiggly
{
    public static partial class Db
    {
        public static void Spawn()
        {
            using var ctx = new SquigglyContext();
            var sw = Stopwatch.StartNew();
            var names = new string[] { "Aide", "Msgr", "Messenger", "King", "WarGhost", "GameBird", "FlyingRooster", "BlueCooer", "Guard" };
            foreach (var spawn in ctx.cq_generator)
            {
                var amount = spawn.maxnpc;

                var cqMob = ctx.cq_monstertype.FirstOrDefault(x => x.id == spawn.npctype);
                if (cqMob == null)
                    continue;

                if (names.Any(cqMob.name.Trim().Contains))
                    amount = spawn.maxnpc;
                if (names.Contains(cqMob.name.Trim()))
                    amount = spawn.maxnpc;

                if (cqMob.name.Trim() == "HawKing")
                    amount = (ushort)(amount * 2);

                if (cqMob.id == 900 || cqMob.id == 910 || cqMob.id == 911)
                    amount = 1;

                for (var i = 0; i < amount; i++)
                {
                    var prefab = cqMob;
                    ref var obj = ref NttWorld.CreateEntity(EntityType.Monster);

                    var spw = new SpawnComponent(obj.Id, (int)spawn.id);
                    var cqm = new CqMonsterComponent(obj.Id, prefab.id);
                    var pos = new PositionComponent(obj.Id, new Vector2(spawn.bound_x, spawn.bound_y), spawn.mapid);
                    var bdy = new BodyComponent(obj.Id, prefab.lookface, (Direction)Random.Shared.Next(0, 9));
                    var hp = new HealthComponent(obj.Id, prefab.life, prefab.life);
                    var ntc = new NameTagComponent(obj.Id, prefab.name.Trim());
                    var vwp = new ViewportComponent(obj.Id, 40f);
                    var inv = new InventoryComponent(obj.Id, prefab.drop_money, 0);

                    if (prefab.action != 0)
                    {
                        var cq = new CqActionComponent(obj.Id, prefab.action);
                        obj.Set(ref cq);
                    }

                    pos.Position.X = (ushort)Random.Shared.Next(spawn.bound_x - 10, spawn.bound_x + spawn.bound_cx + 10);
                    pos.Position.Y = (ushort)Random.Shared.Next(spawn.bound_y - 10, spawn.bound_y + spawn.bound_cy + 10);

                    if (prefab.lookface == 900 || prefab.lookface == 910)
                    {
                        pos.Position = new Vector2(spawn.bound_x, spawn.bound_y);
                        var grd = new GuardPositionComponent(obj.Id, pos.Position);
                        obj.Set(ref grd);
                    }

                    vwp.Viewport.X = pos.Position.X;
                    vwp.Viewport.Y = pos.Position.Y;
                    obj.Set(ref spw);
                    obj.Set(ref pos);
                    obj.Set(ref bdy);
                    obj.Set(ref hp);
                    obj.Set(ref ntc);
                    obj.Set(ref vwp);
                    obj.Set(ref inv);
                    obj.Set(ref cqm);

                    var brn = new BrainComponent(obj.Id);
                    obj.Set(ref brn);

                    if (!Collections.SpatialHashs.ContainsKey(pos.Map))
                    {
                        if (!Collections.Maps.TryGetValue(pos.Map, out var map))
                            continue;

                        Collections.SpatialHashs[pos.Map] = new SpatialHash(10);//new Grid(map.Width, map.Height, 10, 10);
                    }
                    Collections.SpatialHashs[pos.Map].Add(in obj);
                    // if(!Game.Grids.TryGetValue(pos.Map, out var grid))
                    // {
                    //     if (!Collections.Maps.TryGetValue(pos.Map, out var map))
                    //         continue;

                    //     grid = new Grid(map.Width, map.Height, 10, 10);
                    //     Game.Grids[pos.Map] = grid;
                    // }
                }
            }
            sw.Stop();
        }

        public static void LoadCqNpc()
        {
            var sw = Stopwatch.StartNew();
            using var db = new SquigglyContext();
            foreach (var cq in db.cq_npc)
                Collections.CqNpc.TryAdd(cq.id, cq);
            sw.Stop();
            FConsole.WriteLine($"Loaded {Collections.CqNpc.Count} CqNpcs in {sw.ElapsedMilliseconds}ms");
        }
        public static void LoadCqAction()
        {
            var sw = Stopwatch.StartNew();
            using var db = new SquigglyContext();
            foreach (var cq in db.cq_action)
                Collections.CqAction.TryAdd(cq.id, cq);
            sw.Stop();
            FConsole.WriteLine($"Loaded {Collections.CqAction.Count} CqActions in {sw.ElapsedMilliseconds}ms");
        }
        public static void LoadCqTask()
        {
            var sw = Stopwatch.StartNew();
            using var db = new SquigglyContext();
            foreach (var cq in db.cq_task)
                Collections.CqTask.TryAdd(cq.id, cq);
            sw.Stop();
            FConsole.WriteLine($"Loaded {Collections.CqTask.Count} CqTasks in {sw.ElapsedMilliseconds}ms");
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
                    if (!Collections.SpatialHashs.TryGetValue((ushort)cqmap.id, out var _))
                    {
                        var grid = new SpatialHash(10);//new Grid(cqmap.Width, cqmap.Height, 10, 10);
                        Collections.SpatialHashs.Add((ushort)cqmap.id, grid);
                    }
                    // if(!Game.Grids.TryGetValue((ushort)cqmap.id, out var _))
                    // {
                    //     var grid = new Grid(cqmap.Width, cqmap.Height, 10, 10);
                    //     Game.Grids.Add((ushort)cqmap.id, grid);
                    // }
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

                    var ntt = NttWorld.CreateEntityWithNetId(EntityType.Npc, (int)cqNpc.id);
                    var pos = new PositionComponent(ntt.Id, new Vector2(cqNpc.cellx, cqNpc.celly), cqNpc.mapid);
                    var bdy = new BodyComponent(ntt.Id, cqNpc.lookface);
                    var npcc = new NpcComponent(ntt.Id, npc.Base, npc.Type, npc.Sort);
                    var vwp = new ViewportComponent(ntt.Id, 40);
                    ntt.Set(ref pos);
                    ntt.Set(ref bdy);
                    ntt.Set(ref npcc);
                    ntt.Set(ref vwp);

                    if (!Collections.SpatialHashs.ContainsKey(pos.Map))
                    {
                        if (!Collections.Maps.TryGetValue(pos.Map, out var _))
                            continue;
                        // 
                        Collections.SpatialHashs[pos.Map] = new SpatialHash(10);//new Grid(map.Width, map.Height, 10, 10);
                    }
                    Collections.SpatialHashs[pos.Map].Add(in ntt);
                    // if(!Game.Grids.TryGetValue(pos.Map, out var grid))
                    // {
                    //     if (!Collections.Maps.TryGetValue(pos.Map, out var map))
                    //         continue;

                    //     grid = new Grid(map.Width, map.Height, 10, 10);
                    //     Game.Grids[pos.Map] = grid;
                    // }
                }
            }
            sw.Stop();
            FConsole.WriteLine($"[SquigglyLite] Loaded \t Npcs in {sw.Elapsed.TotalMilliseconds}ms");
        }

        public static void LoadLevelExp()
        {
            var sw = Stopwatch.StartNew();
            Collections.LevelExps.LoadFromDat("CLIENT_FILES/LevelExp.dat");
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
        public static unsafe void LoadDatFiles()
        {
            var Cipher = new COFAC();
            string TmpFile = Path.GetTempFileName();
            Cipher.GenerateKey(0x2537);

            using (FileStream Reader = new("CLIENT_FILES/itemtype.dat", FileMode.Open, FileAccess.Read, FileShare.Read))
            using (FileStream Writer = new(TmpFile, FileMode.Open, FileAccess.Write, FileShare.Read))
            {
                var Buffer = new byte[10240];

                var Length = Reader.Read(Buffer, 0, Buffer.Length);
                while (Length > 0)
                {
                    fixed (byte* pBuffer = Buffer)
                        Cipher.Decrypt(pBuffer, Length);
                    Writer.Write(Buffer, 0, Length);

                    Length = Reader.Read(Buffer, 0, Buffer.Length);
                }
            }

            Collections.ItemType.LoadFromTxt(TmpFile);
            File.Delete(TmpFile);
            Collections.MagicType.LoadFromDat("CLIENT_FILES/MagicType.dat");

            FConsole.WriteLine($"{Collections.MagicType.Count} magic types loaded.");
            FConsole.WriteLine($"{Collections.ItemType.Count} item types loaded.");
        }
    }
}