using System.Net.Sockets;
using System.Text;
using Co2Core.Security.Cryptography;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Helpers;
using MagnumOpus.Networking;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Simulation.Systems;
using MagnumOpus.SpacePartitioning;
using MagnumOpus.Squiggly;

namespace MagnumOpus
{
    public static class Game
    {
        internal static readonly Dictionary<int, SpatialHash> SpatialHashs = new();
        private static readonly TcpListener GameListener = new(System.Net.IPAddress.Any, 5816);
        private static readonly TcpListener LoginListener = new(System.Net.IPAddress.Any, 9958);

        private static unsafe void Main()
        {
            var systems = new List<NttSystem>
            {
                new PacketsIn(),
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
            SquigglyDb.LoadShopDat("CLIENT_FILES/Shop.dat");
            SquigglyDb.LoadMaps();
            SquigglyDb.LoadPortals();
            SquigglyDb.LoadLevelExp();
            SquigglyDb.LoadItemBonus();
            SquigglyDb.LoadCqAction();
            SquigglyDb.LoadCqTask();
            SquigglyDb.LoadCqNpc();
            SquigglyDb.Spawn();
            SquigglyDb.LoadNpcs();

            NttWorld.SetSystems(systems.ToArray());
            NttWorld.SetTPS(60);
            NttWorld.RegisterOnSecond(() =>
            {
                var lines = PerformanceMetrics.Draw();
                var linesArr = lines.Split('\r', '\n');
                FConsole.WriteLine(lines);

                for (int i = 0; i < linesArr.Length; i++)
                {
                    foreach (var player in NttWorld.Players)
                    {
                        if (i == 0)
                        {
                            var msgUp = MsgText.Create(player, linesArr[i], Enums.MsgTextType.MiniMap);
                            player.NetSync(ref msgUp);
                            continue;
                        }

                        var msg = MsgText.Create(player, linesArr[i], Enums.MsgTextType.MiniMap2);
                        player.NetSync(ref msg);
                    }
                }

                PerformanceMetrics.Restart();
            });

            FConsole.WriteLine($"[LOGIN] Listening on port {9958}...");
            LoginListener.Start();
            FConsole.WriteLine($"[GAME] Listening on port {5816}...");
            GameListener.Start();

            var loginThread = new Thread(LoginServerLoop) { IsBackground = true, Priority = ThreadPriority.Highest };
            var gameThread = new Thread(GameServerLoop) { IsBackground = true, Priority = ThreadPriority.Highest };
            loginThread.Start();
            gameThread.Start();

            while (true)
                NttWorld.Update();
        }

        private static void LoginServerLoop()
        {
            var ready = false;
            NttWorld.RegisterOnTick(() => { ready = true; });
            while (true)
            {
                var client = LoginListener.AcceptTcpClient();
                while(!ready);

                var player = NttWorld.CreateEntity(EntityType.Player);
                var net = new NetworkComponent(in player, client.Client);
                player.Set(ref net);

                var ipendpoint = client.Client.RemoteEndPoint?.ToString();
                if (ipendpoint == null)
                    break;

                IpRegistry.Register(player, ipendpoint.Split(':')[0]);
                FConsole.WriteLine($"[LOGIN] Client connected: {client.Client.RemoteEndPoint}");

                var count = net.Socket.Receive(net.RecvBuffer.Span[..52]);
                var packet = net.RecvBuffer[..count];
                net.AuthCrypto.Decrypt(packet.Span, packet.Span);

                LoginPacketHandler.Process(in player, in packet);

                new Thread(() => LoginClientLoop(in player)).Start();
            }
        }

        private static void LoginClientLoop(in NTT player)
        {
            ref var net = ref player.Get<NetworkComponent>();
            try
            {
                while (net.Socket.Connected)
                {
                    var count = net.Socket.Receive(net.RecvBuffer.Span);
                    if (count == 0)
                        break;
                    var packet = net.RecvBuffer[..count];
                    net.AuthCrypto.Decrypt(packet.Span, packet.Span);
                    LoginPacketHandler.Process(in player, in packet);
                }
            }
            catch
            {
                FConsole.WriteLine($"[LOGIN] Client disconnected");
                net.Socket.Close();
                net.Socket.Dispose();
            }
        }

        private static void GameServerLoop()
        {
            while (true)
            {
                var client = GameListener.AcceptTcpClient();

                FConsole.WriteLine($"[GAME] Client connected: {client.Client.RemoteEndPoint}");

                var ipendpoint = client.Client.RemoteEndPoint?.ToString();
                if (ipendpoint == null)
                    break;

                var (found, ntt) = IpRegistry.GetEntity(ipendpoint.Split(':')[0]);
                if (!found)
                    continue;

                ref var net = ref ntt.Get<NetworkComponent>();
                net.UseGameCrypto = true;
                net.Socket.Close();
                net.Socket.Dispose();
                net.Socket = client.Client;

                net.DiffieHellman.ComputePublicKeyAsync();
                var dhx = MsgDHX.Create(net.ClientIV, net.ServerIV, Networking.Cryptography.DiffieHellman.P, Networking.Cryptography.DiffieHellman.G, net.DiffieHellman.GetPublicKey());
                ntt.NetSync(ref dhx);

                var count = net.Socket.Receive(net.RecvBuffer.Span);
                var packet = net.RecvBuffer[..count];
                net.GameCrypto.Decrypt(packet.Span);

                var packetSpan = packet.Span;

                var size = BitConverter.ToUInt16(packetSpan[7..]);
                var junkSize = BitConverter.ToInt32(packetSpan[11..]);
                var pkSize = BitConverter.ToInt32(packetSpan[(15 + junkSize)..]);
                var pk = new byte[pkSize];
                for (var i = 0; i < pkSize; i++)
                    pk[i] = packetSpan[19 + junkSize + i];

                var pubkey = Encoding.ASCII.GetString(pk);
                net.DiffieHellman.ComputePrivateKey(pubkey);
                net.GameCrypto.GenerateKeys(net.DiffieHellman.GetPrivateKey());
                net.GameCrypto.SetIVs(net.ServerIV, net.ClientIV);

                new Thread(() => GameClientLoop(in ntt)).Start();
            }
        }
        private static void GameClientLoop(in NTT ntt)
        {
            ref var net = ref ntt.Get<NetworkComponent>();
            var buffer = net.RecvBuffer.Span;
            var crypto = net.GameCrypto;

            while (true)
            {
                try
                {
                    // Receive the packet size.
                    var sizeBytes = buffer[..2];
                    var count = net.Socket.Receive(sizeBytes);
                    if (count != 2)
                        throw new Exception("NO DIE");

                    // Decrypt the size bytes and compute the packet size.
                    crypto.Decrypt(sizeBytes);
                    var size = BitConverter.ToUInt16(sizeBytes) + 8;

                    // Keep receiving until the entire packet is received.
                    count = 2;
                    while (count < size)
                    {
                        var received = net.Socket.Receive(buffer[count..size]);
                        if (received == 0)
                            throw new Exception("NO DIE");

                        count += received;
                    }

                    // Process the packet.
                    crypto.Decrypt(buffer[2..size]);
                    net.RecvQueue.Enqueue(buffer[..size].ToArray());
                }
                catch
                {
                    FConsole.WriteLine($"[GAME] Client disconnected: {net.Username}");
                    net.Socket?.Close();
                    net.Socket?.Dispose();
                    NttWorld.Destroy(in ntt);
                    break;
                }
            }

            NttWorld.Destroy(in ntt);
        }
    }
}