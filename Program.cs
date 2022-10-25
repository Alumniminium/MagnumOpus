using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime;
using System.Text;
using MagnumOpus.ECS;
using MagnumOpus.Helpers;
using MagnumOpus.Networking;
using MagnumOpus.Networking.Cryptography;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Simulation.Components;
using MagnumOpus.Simulation.Systems;
using SpaceParitioning;

internal class Game
{
    public const int TargetTps = 30;
    public static uint CurrentTick;
    public static Dictionary<int, Grid> Grids = new();

    private const string SLEEP = "Sleep";
    private const string WORLD_UPDATE = "World.Update";

    private static void Main(string[] args)
    {
        var systems = new List<PixelSystem>
            {
                new SpawnSystem(),
                new LifetimeSystem(),
                new ViewportSystem(),
                new InputSystem(),
                new DamageSystem(),
                new HealthSystem(),
                new DropSystem(),
                new DeathSystem(),
                new LevelExpSystem(),
                new RespawnSystem(),
                new NetSyncSystem(),
            };
        PixelWorld.Systems = systems.ToArray();

        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
        PerformanceMetrics.RegisterSystem(WORLD_UPDATE);
        PerformanceMetrics.RegisterSystem(SLEEP);
        PerformanceMetrics.RegisterSystem(nameof(Game));

        // Db.LoadBaseResources();
        // SpawnManager.Respawn();
        var worker = new Thread(SimulationLoopAsync) { IsBackground = true, Priority = ThreadPriority.Highest };
        var loginThread = new Thread(LoginServerLoop) { IsBackground = true, Priority = ThreadPriority.Highest };
        var gameThread = new Thread(GameServerLoop) { IsBackground = true, Priority = ThreadPriority.Highest };
        worker.Start();
        loginThread.Start();
        gameThread.Start();
        while (true)
        {
            Thread.Sleep(1000);
            //var lines = PerformanceMetrics.Draw();
            //Console.WriteLine(lines);
        }
    }

    private static void LoginServerLoop()
    {
        TcpListener listener = new(System.Net.IPAddress.Any, 9958);
        listener.Start();
        while (true)
        {
            var client = listener.AcceptTcpClient();
            var stream = client.GetStream();
            var reader = new BinaryReader(stream);
            // var writer = new BinaryWriter(stream);

            var player = PixelWorld.CreateEntity(EntityType.Player);
            var net = new NetworkComponent(player, client.Client);
            player.Add(ref net);

            var ipendpoint = client.Client.RemoteEndPoint?.ToString();
            if (ipendpoint == null)
                break;

            IpRegistry.Register(player, ipendpoint.Split(':')[0]);
            Console.WriteLine($"[LOGIN] Client connected: {client.Client.RemoteEndPoint}");

            var count = reader.Read(net.RecvBuffer.Span[..52]);
            var packet = net.RecvBuffer[..count];
            net.AuthCrypto.Decrypt(packet.Span, packet.Span);
            LoginPacketHandler.Process(in player, in packet);
            
            try{
            while (client.Connected)
            {
                count = reader.Read(net.RecvBuffer.Span);
                if (count == 0)
                    break;
                packet = net.RecvBuffer[..count];
                net.AuthCrypto.Decrypt(packet.Span, packet.Span);
                LoginPacketHandler.Process(in player, in packet);
            }
            }
            catch
            {
                Console.WriteLine($"[LOGIN] Client disconnected: {client.Client.RemoteEndPoint}");
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

            Console.WriteLine($"[GAME] Client connected: {client.Client.RemoteEndPoint}");

            var ipendpoint = client.Client.RemoteEndPoint?.ToString();
            if (ipendpoint == null)
                break;

            var (found, player) = IpRegistry.GetEntity(ipendpoint.Split(':')[0]);
            if (!found)
                continue;

            ref var net = ref player.Get<NetworkComponent>();
            net.Socket.Close();
            net.Socket.Dispose();
            net.Socket = client.Client;

            net.DiffieHellman.ComputePublicKeyAsync();
            Memory<byte> dhx = MsgDHX.Create(net.ClientIV, net.ServerIV, DiffieHellman.P, DiffieHellman.G, net.DiffieHellman.GetPublicKey());

            net.GameCrypto.Encrypt(dhx.Span, dhx.Span);
            player.NetSync(dhx);

            var count = net.Socket.Receive(net.RecvBuffer.Span);
            var packet = net.RecvBuffer[..count];
            net.GameCrypto.Decrypt(packet.Span, packet.Span);

            var pubkey = Encoding.ASCII.GetString(ParseResponse(packet));
            Console.WriteLine(GamePacketHandler.Dump(packet.ToArray()));
            Console.WriteLine($"Pubkey: {pubkey}");

            net.DiffieHellman.ComputePrivateKey(pubkey);
            net.GameCrypto.GenerateKeys(net.DiffieHellman.GetPrivateKey());
            net.GameCrypto.SetIVs(net.ServerIV, net.ClientIV);

            while (true)
            {
                count = net.Socket.Receive(net.RecvBuffer.Span[..2]);
     
                if (count == 0)
                    break;

                net.GameCrypto.Decrypt(net.RecvBuffer.Span[..2],net.RecvBuffer.Span[..2]);
                var size = BitConverter.ToUInt16(net.RecvBuffer.Span[..2])+8;

                while(count < size)
                {
                    var received = net.Socket.Receive(net.RecvBuffer.Span[count..size]);
                    count += received;

                    if (received == 0)
                        break;
                }
                
                packet = net.RecvBuffer[..size];
                net.GameCrypto.Decrypt(packet.Span[2..size], packet.Span[2..size]);
                GamePacketHandler.Process(in player, packet);
            }
        }
    }

    private static unsafe byte[] ParseResponse(Memory<byte> buffer)
    {
        byte[] publicKey;
        fixed (byte* ptr = buffer.Span)
        {
            var length = *(ushort*)(ptr + 7);
            Console.WriteLine("DH Offset 7: " + length + " | Buffer Size: " + buffer.Length);
            Console.WriteLine("DH Length: " + (length + 7) + " | Buffer Size: " + buffer.Length);
            var junkLength = *(int*)(ptr + 11);
            Console.WriteLine("DH Junk Length: " + junkLength);
            var publicKeyLength = *(int*)(ptr + 15 + junkLength);
            Console.WriteLine("DH Key Length: " + publicKeyLength);

            publicKey = new byte[publicKeyLength];
            for (var i = 0; i < publicKeyLength; i++)
                publicKey[i] = *(ptr + 19 + junkLength + i);
        }

        return publicKey;
    }

    private static async void SimulationLoopAsync()
    {
        var sw = Stopwatch.StartNew();
        var fixedUpdateAcc = 0f;
        const float fixedUpdateTime = 1f / TargetTps;
        var onSecond = 0f;

        while (true)
        {
            double last;
            var dt = MathF.Min(1f / TargetTps, (float)sw.Elapsed.TotalSeconds);
            fixedUpdateAcc += dt;
            onSecond += dt;
            sw.Restart();

            IncomingPacketQueue.ProcessAll();

            if (fixedUpdateAcc >= fixedUpdateTime)
            {
                for (var i = 0; i < PixelWorld.Systems.Length; i++)
                {
                    var system = PixelWorld.Systems[i];
                    last = sw.Elapsed.TotalMilliseconds;
                    system.Update(fixedUpdateTime);
                    PerformanceMetrics.AddSample(system.Name, sw.Elapsed.TotalMilliseconds - last);
                    last = sw.Elapsed.TotalMilliseconds;
                    PixelWorld.Update();
                    PerformanceMetrics.AddSample(WORLD_UPDATE, sw.Elapsed.TotalMilliseconds - last);
                }


                if (onSecond >= 1)
                {
                    PerformanceMetrics.Restart();
                    onSecond = 0;
                }

                fixedUpdateAcc -= fixedUpdateTime;
                CurrentTick++;
                PerformanceMetrics.AddSample(nameof(Game), sw.Elapsed.TotalMilliseconds);
            }
            await OutgoingPacketQueue.SendAll();

            Sleep(sw, fixedUpdateTime);
        }
    }

    private static void Sleep(Stopwatch sw, float fixedUpdateTime)
    {
        double last;
        var tickTime = sw.Elapsed.TotalMilliseconds;
        last = sw.Elapsed.TotalMilliseconds;
        var sleepTime = (int)Math.Max(0, (fixedUpdateTime * 1000) - tickTime);
        Thread.Sleep(sleepTime);
        PerformanceMetrics.AddSample(SLEEP, sw.Elapsed.TotalMilliseconds - last);
    }
}