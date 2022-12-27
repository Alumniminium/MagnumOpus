using System.Net.Sockets;
using System.Text;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Helpers;
using MagnumOpus.Networking;
using MagnumOpus.Networking.Cryptography;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Simulation.Systems;
using MagnumOpus.Squiggly;
using SpacePartitioning;

namespace MagnumOpus
{
    public static class Game
    {
        internal static readonly Dictionary<int, Grid> Grids = new();
        private static readonly TcpListener GameListener = new(System.Net.IPAddress.Any, 5816);
        private static readonly TcpListener LoginListener = new(System.Net.IPAddress.Any, 9958);

        private static void Main()
        {
            var systems = new List<PixelSystem>
            {
                new WalkSystem(),
                new JumpSystem(),
                new PortalSystem(),
                new ViewportSystem(),
                new BasicAISystem(),
                new AttackSystem(),
                new DamageSystem(),
                new HealthSystem(),
                new ExpRewardSystem(),
                new DeathSystem(),
            };
            SquigglyDb.LoadMaps();
            SquigglyDb.LoadPortals();
            SquigglyDb.LoadLevelExp();
            SquigglyDb.LoadItemBonus();
            SquigglyDb.LoadMobs();
            SquigglyDb.LoadSpawns();
            SquigglyDb.Spawn();
            SquigglyDb.LoadNpcs();

            PixelWorld.SetSystems(systems.ToArray());
            PixelWorld.SetTPS(30);
            PixelWorld.RegisterOnSecond(() =>
            {
                var lines = PerformanceMetrics.Draw();
                // Console.WriteLine(lines);
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
                PixelWorld.Update();
        }

        private static void LoginServerLoop()
        {
            while (true)
            {
                var client = LoginListener.AcceptTcpClient();
                client.Client.NoDelay = true;
                client.Client.DontFragment = true;

                var player = PixelWorld.CreateEntity(EntityType.Player);
                var net = new NetworkComponent(in player, client.Client);
                player.Add(ref net);

                var ipendpoint = client.Client.RemoteEndPoint?.ToString();
                if (ipendpoint == null)
                    break;

                IpRegistry.Register(player, ipendpoint.Split(':')[0]);
                FConsole.WriteLine($"[LOGIN] Client connected: {client.Client.RemoteEndPoint}");

                var count = net.Socket.Receive(net.RecvBuffer.Span[..52]);
                var packet = net.RecvBuffer[..count];
                net.AuthCrypto.Decrypt(packet.Span, packet.Span);
                LoginPacketHandler.Process(in player, in packet);

                new Thread(() => NewMethod(in player)).Start();
            }
        }

        private static void NewMethod(in PixelEntity player)
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
                FConsole.WriteLine($"[LOGIN] Client disconnected: {net.Socket.RemoteEndPoint}");
                net.Socket.Close();
                net.Socket.Dispose();
            }
        }

        private static void GameServerLoop()
        {
            while (true)
            {
                var client = GameListener.AcceptTcpClient();
                client.Client.NoDelay = true;
                client.Client.DontFragment = true;

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
                var dhx = MsgDHX.Create(net.ClientIV, net.ServerIV, DiffieHellman.P, DiffieHellman.G, net.DiffieHellman.GetPublicKey());
                ntt.NetSync(ref dhx);

                var count = net.Socket.Receive(net.RecvBuffer.Span);
                var packet = net.RecvBuffer[..count];
                net.GameCrypto.Decrypt(packet.Span);

                unsafe
                {
                    byte[] pk;
                    fixed (byte* ptr = packet.Span)
                    {
                        var size = *(ushort*)(ptr + 7);
                        var junkSize = *(int*)(ptr + 11);
                        var pkSize = *(int*)(ptr + 15 + junkSize);
                        pk = new byte[pkSize];

                        pk = new byte[pkSize];
                        for (var i = 0; i < pkSize; i++)
                            pk[i] = *(ptr + 19 + junkSize + i);
                    }

                    var pubkey = Encoding.ASCII.GetString(pk);
                    net.DiffieHellman.ComputePrivateKey(pubkey);
                    net.GameCrypto.GenerateKeys(net.DiffieHellman.GetPrivateKey());
                    net.GameCrypto.SetIVs(net.ServerIV, net.ClientIV);
                }

                new Thread(() => NewMethod1(in ntt)).Start();
            }
        }

        private static void NewMethod1(in PixelEntity ntt)
        {
            ref var net = ref ntt.Get<NetworkComponent>();
            while (true)
            {
                var count = net.Socket.Receive(net.RecvBuffer.Span[..2]);

                if (count == 0)
                    break;

                net.GameCrypto.Decrypt(net.RecvBuffer.Span[..2]);
                var size = BitConverter.ToUInt16(net.RecvBuffer.Span[..2]) + 8;

                while (count < size)
                {
                    var received = net.Socket.Receive(net.RecvBuffer.Span[count..size]);
                    count += received;

                    if (received == 0)
                        break;
                }

                var packet = net.RecvBuffer[..size];
                net.GameCrypto.Decrypt(packet.Span[2..size]);
                IncomingPacketQueue.Add(in ntt, in packet);
            }
        }
    }
}