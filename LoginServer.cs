using System.Net.Sockets;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Helpers;
using MagnumOpus.Networking;

namespace MagnumOpus
{
    public static class LoginServer
    {
        private static readonly TcpListener LoginListener = new(System.Net.IPAddress.Any, 9958);
        private static readonly Thread loginThread;

        static LoginServer()
        {
            loginThread = new Thread(LoginServerLoop) { IsBackground = true, Priority = ThreadPriority.Highest };
            loginThread.Start();
        }

        public static void Start()
        {
            FConsole.WriteLine($"[GAME] Listening on port {9958}...");
            LoginListener.Start();
        }

        private static void LoginServerLoop()
        {
            var ready = false;
            NttWorld.RegisterOnEndTick(() => { ready = true; });
            while (true)
            {
                var client = LoginListener.AcceptTcpClient();
                while (!ready) ;

                var player = NttWorld.CreateEntity(EntityType.Player);
                var net = new NetworkComponent(client.Client);
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
    }
}