using System.Diagnostics;
using System.Runtime;
using HerstLib.IO;
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
            NttWorld.SetSystems(systems.ToArray());
            NttWorld.SetTPS(30);
            ReflectionHelper.GetRemoveMethods();
            ref var ntt = ref NttWorld.GetEntity(0);
            ntt.Recycle();
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


            LoginServer.Start();
            GameServer.Start();


            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

            while (true)
            {
                NttWorld.Update();

                if (!Console.KeyAvailable)
                    continue;

                var input = Console.ReadKey();
                if (input.Key == ConsoleKey.S)
                {
                    FConsole.WriteLine("[SERVER] Saving...");
                    var ts = Stopwatch.GetTimestamp();
                    ReflectionHelper.SaveComponents("_STATE_FILES");
                    NttWorld.Save("_STATE_FILES");
                    IdGenerator.Save("_STATE_FILES");
                    var ms = Stopwatch.GetElapsedTime(ts).TotalMilliseconds;
                    FConsole.WriteLine($"[SERVER] Saved in {ms:0.00}ms");
                }
            }
        }
    }
}