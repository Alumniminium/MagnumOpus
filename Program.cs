﻿using System.Numerics;
using System.Runtime;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Helpers;
using MagnumOpus.Squiggly;
using MagnumOpus.Systems;

namespace MagnumOpus
{
    public static class Game
    {
        private static unsafe void Main()
        {
            //handle unhanded exceptions
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var e = (Exception)args.ExceptionObject;
                FConsole.WriteLine($"[FATAL] {e.Message}");
                FConsole.WriteLine(e.InnerException?.Message ?? string.Empty);
                FConsole.WriteLine(e.StackTrace ?? string.Empty);
            };

            Constants.PrometheusPort = ushort.TryParse(Environment.GetEnvironmentVariable("PROMETHEUS_PORT"), out var p) ? p : (ushort)1234;
            Constants.LoginPort = ushort.TryParse(Environment.GetEnvironmentVariable("LOGIN_PORT"), out var p2) ? p2 : (ushort)9958;
            Constants.GamePort = ushort.TryParse(Environment.GetEnvironmentVariable("GAME_PORT"), out var p3) ? p3 : (ushort)5816;
            Constants.ServerIP = Environment.GetEnvironmentVariable("PUBLIC_IP") ?? "192.168.0.209";

            // using var server = new Prometheus.MetricServer(port: Constants.PrometheusPort);
            // server.Start();

            var systems = new List<NttSystem>
            {
                new PacketsIn(),
                new MonsterRespawnSystem(),
                new BasicAISystem(),
                new GuardAISystem(),
                new BoidSystem(),
                new WalkSystem(),
                new JumpSystem(),
                new EmoteSystem(),
                new PortalSystem(),
                new TeleportSystem(),
                new SpatialHashSystem(),

                new ViewportSystem(),

                new MagicAttackRoutingSystem(),
                new TargetFinderSystem(),
                new MagicAttackSystem(),
                new AttackSystem(),
                new DamageSystem(),
                new TeamSystem(),
                new ExpRewardSystem(),
                new LifetimeSystem(),
                new DropItemSystem(),
                new DropMoneySystem(),
                new PickupSystem(),
                new ItemUseSystem(),
                new ReviveSystem(),
                new ShopSystem(),
                new EquipSystem(),
                new DeathSystem(),
                new DestroySystem(),
                new PacketsOut(),
            };

            FConsole.WriteLine("[DATABASE] Loading...");
            NttWorld.SetSystems(systems.ToArray());
            NttWorld.SetTPS(60);
            ReflectionHelper.LoadComponents("_STATE_FILES");
            ref var ntt = ref NttWorld.GetEntity(0);
            ntt.Recycle();

            Db.LoadDatFiles();
            Db.LoadShopDat("CLIENT_FILES/Shop.dat");
            Db.LoadMaps();
            Db.LoadPortals();
            Db.LoadLevelExp();
            Db.LoadItemBonus();
            Db.LoadCqMonsterType();
            Db.LoadCqAction();
            Db.LoadCqTask();
            Db.LoadCqNpc();
            Db.LoadCqPointAllot();
            Db.LoadSpawners();
            Db.LoadNpcs();

            foreach (var worldNtt in NttWorld.NTTs)
            {
                NttWorld.InformChangesFor(worldNtt.Value);
                ref var pos = ref worldNtt.Value.Get<PositionComponent>();
                if (pos.Position == default)
                    continue;

                var shr = new SpatialHashUpdateComponent(pos.Position, Vector2.Zero, pos.Map, pos.Map, SpacialHashUpdatType.Add);
                worldNtt.Value.Set(ref shr);
            }

            LoginServer.Start();
            GameServer.Start();

            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

            while (true)
            {
                NttWorld.Update();
                // if (Debugger.IsAttached)
                // {
                //     if (!Console.KeyAvailable)
                //         continue;

                //     var input = Console.ReadKey();
                //     if (input.Key == ConsoleKey.S)
                //     {
                //         FConsole.WriteLine("[SERVER] Saving...");
                //         var ts = Stopwatch.GetTimestamp();
                //         ReflectionHelper.SaveComponents("_STATE_FILES");
                //         NttWorld.Save("_STATE_FILES");
                //         IdGenerator.Save("_STATE_FILES");
                //         var ms = Stopwatch.GetElapsedTime(ts).TotalMilliseconds;
                //         FConsole.WriteLine($"[SERVER] Saved in {ms:0.00}ms");
                //     }
                // }
            }
        }
    }
}