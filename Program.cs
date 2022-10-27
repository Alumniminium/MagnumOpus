using System.Net.Sockets;
using System.Text;
using HerstLib.IO;
using MagnumOpus.ECS;
using MagnumOpus.Helpers;
using MagnumOpus.Networking;
using MagnumOpus.Networking.Cryptography;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Simulation.Components;
using MagnumOpus.Simulation.Systems;
using SpacePartitioning;

namespace MagnumOpus
{
    internal class Game
    {
        public static Dictionary<int, Grid> Grids = new();

        private static void Main()
        {
            var systems = new List<PixelSystem>
            {
                new SpawnSystem(), new LifetimeSystem(),
                new WalkSystem(), new JumpSystem(),
                new ViewportSystem(), new InputSystem(),
                new DamageSystem(), new HealthSystem(),
                new DropSystem(), new DeathSystem(),
                new LevelExpSystem(), new RespawnSystem(),
                new NetSyncSystem(),
            };
            ConquerWorld.SetSystems(systems.ToArray());
            ConquerWorld.SetTPS(30);
            ConquerWorld.RegisterOnSecond(() =>
            {
                var lines = PerformanceMetrics.Draw();
                Console.WriteLine(lines);
                PerformanceMetrics.Restart();
            });

            var loginThread = new Thread(LoginServerLoop) { IsBackground = true, Priority = ThreadPriority.Highest };
            var gameThread = new Thread(GameServerLoop) { IsBackground = true, Priority = ThreadPriority.Highest };
            loginThread.Start();
            gameThread.Start();

            while (true)
                ConquerWorld.Update();
        }

        private static void LoginServerLoop()
        {
            TcpListener listener = new(System.Net.IPAddress.Any, 9958);
            listener.Start();
            while (true)
            {
                var client = listener.AcceptTcpClient();
                client.Client.NoDelay = true;
                client.Client.DontFragment = true;

                var player = ConquerWorld.CreateEntity(EntityType.Player);
                var net = new NetworkComponent(player, client.Client);
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

                try
                {
                    while (client.Connected)
                    {
                        count = net.Socket.Receive(net.RecvBuffer.Span);
                        if (count == 0)
                            break;
                        packet = net.RecvBuffer[..count];
                        net.AuthCrypto.Decrypt(packet.Span, packet.Span);
                        LoginPacketHandler.Process(in player, in packet);
                    }
                }
                catch
                {
                    FConsole.WriteLine($"[LOGIN] Client disconnected: {client.Client.RemoteEndPoint}");
                    client.Close();
                    client.Dispose();
                }
            }
        }
        private static void GameServerLoop()
        {
            TcpListener listener = new(System.Net.IPAddress.Any, 5816);
            listener.Start();
            while (true)
            {
                var client = listener.AcceptTcpClient();
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
                Memory<byte> dhx = MsgDHX.Create(net.ClientIV, net.ServerIV, DiffieHellman.P, DiffieHellman.G, net.DiffieHellman.GetPublicKey());
                ntt.NetSync(in dhx);

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
                    FConsole.WriteLine(packet.Dump());
                    FConsole.WriteLine($"Pubkey: {pubkey}");

                    net.DiffieHellman.ComputePrivateKey(pubkey);
                    net.GameCrypto.GenerateKeys(net.DiffieHellman.GetPrivateKey());
                    net.GameCrypto.SetIVs(net.ServerIV, net.ClientIV);
                }

                while (true)
                {
                    count = net.Socket.Receive(net.RecvBuffer.Span[..2]);

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

                    packet = net.RecvBuffer[..size];
                    net.GameCrypto.Decrypt(packet.Span[2..size]);
                    IncomingPacketQueue.Add(in ntt, in packet);
                }
            }
        }
    }
}