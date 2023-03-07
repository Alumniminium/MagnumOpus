using System.Diagnostics;
using HerstLib.IO;
using MagnumOpus.ECS;
using MagnumOpus.Networking;
using MagnumOpus.Simulation.Systems;
using MagnumOpus.Squiggly;

namespace MagnumOpus
{
    public static class Game
    {
        private static unsafe void Main()
        {
            using var server = new Prometheus.MetricServer(port: 1234);
            server.Start();
            var systems = new List<NttSystem>
            {
                new PacketsIn(),
                new MonsterRespawnSystem(),
                new BasicAISystem(),
                new GuardAISystem(),
                new WalkSystem(),
                new JumpSystem(),
                new EmoteSystem(),
                new PortalSystem(),
                new TeleportSystem(),
                new ViewportSystem(),
                new MagicAttackRoutingSystem(),
                new TargetFinderCircleSystem(),
                new TargetFinderLineSystem(),
                new TargetFinderSectorSystem(),
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
            Db.LoadDatFiles();
            Db.LoadShopDat("CLIENT_FILES/Shop.dat");
            Db.LoadMaps();
            Db.LoadPortals();
            Db.LoadLevelExp();
            Db.LoadItemBonus();
            Db.LoadCqAction();
            Db.LoadCqTask();
            Db.LoadCqNpc();
            Db.LoadCqPointAllot();
            Db.LoadSpawners();
            Db.LoadNpcs();

            NttWorld.SetSystems(systems.ToArray());
            NttWorld.SetTPS(30);
            NttWorld.RegisterOnSecond(() =>
            {
                // var lines = PerformanceMetrics.Draw();
                // var linesArr = lines.Split('\r', '\n');
                // FConsole.WriteLine(lines);

                // for (int i = 0; i < linesArr.Length; i++)
                // {
                //     foreach (var player in NttWorld.Players)
                //     {
                //         if (i == 0)
                //         {
                //             var msgUp = MsgText.Create(player, linesArr[i], Enums.MsgTextType.MiniMap);
                //             player.NetSync(ref msgUp);
                //             continue;
                //         }

                //         var msg = MsgText.Create(player, linesArr[i], Enums.MsgTextType.MiniMap2);
                //         player.NetSync(ref msg);
                //     }
                // }

                // PerformanceMetrics.Restart();
            });

            LoginServer.Start();
            GameServer.Start();

            while (true)
            {
                NttWorld.Update();

                if (Debugger.IsAttached)
                    continue;

                if (!Console.KeyAvailable)
                    continue;

                var input = Console.ReadKey();
                if (input.Key == ConsoleKey.S)
                {
                    FConsole.WriteLine("[SERVER] Saving...");
                    ReflectionHelper.SaveComponents("_STATE_FILES");
                    NttWorld.Save("_STATE_FILES");
                    FConsole.WriteLine("[SERVER] Saved.");
                }
            }
        }
    }
}