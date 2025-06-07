using System.Net.Sockets;
using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Helpers;
using MagnumOpus.Networking;
using NttECS.ECS;

namespace MagnumOpus
{
    public static class LoginServer
    {
        private static readonly TcpListener LoginListener = new(System.Net.IPAddress.Any, Constants.LoginPort);
        private static readonly Thread loginThread = new(AcceptLoop) { IsBackground = true, Priority = ThreadPriority.Highest };

        public static void Start()
        {
            FConsole.WriteLine($"[GAME] Listening on port {Constants.LoginPort}...");
            LoginListener.Start();
            loginThread.Start();
        }

        private static void AcceptLoop()
        {
            var ready = false;
            NttWorld.RegisterOnEndTick(() => { ready = true; });
            while (true)
            {
                var client = LoginListener.AcceptTcpClient();
                while (!ready) ;

                var player = NttWorld.CreateEntity(IdGenerator.GetPlayerId());
                var net = new NetworkComponent(client.Client);
                player.Set(ref net);

                var ipendpoint = client.Client.RemoteEndPoint?.ToString();
                if (ipendpoint == null)
                    break;

                IpRegistry.Register(player, ipendpoint.Split(':')[0]);
                FConsole.WriteLine($"[LOGIN] Client connected: {client.Client.RemoteEndPoint}");
                var buffer = new byte[1024];
                var count = net.Socket.Receive(buffer);
                var packet = buffer[..count];
                net.Crypto.Decrypt(packet.AsSpan(), count);
                LoginPacketHandler.Process(in player, packet);

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
                    var buffer = new byte[1024];
                    var count = net.Socket.Receive(buffer);
                    if (count == 0) break;
                    var packet = buffer[..count];
                    net.Crypto.Decrypt(packet.AsSpan(), count);
                    LoginPacketHandler.Process(in player, packet);
                }
            }
            catch
            {
                FConsole.WriteLine($"[LOGIN] Client disconnected");
                net.Socket.Close();
                net.Socket.Dispose();
            }
        }
    }
}