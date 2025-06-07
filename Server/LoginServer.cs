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

                var player = NttWorld.CreateEntity(EntityType.Player);
                var net = new NetworkComponent(client.Client);
                player.Set(ref net);

                var ipendpoint = client.Client.RemoteEndPoint?.ToString();
                if (ipendpoint == null)
                    break;

                IpRegistry.Register(player, ipendpoint.Split(':')[0]);
                FConsole.WriteLine($"[LOGIN] Client connected: {client.Client.RemoteEndPoint}");

                // Use dedicated buffer for initial login packet
                var initialBuffer = new byte[52];
                var count = net.Socket.Receive(initialBuffer);
                Memory<byte> packet = new byte[count];
                initialBuffer.AsSpan(0, count).CopyTo(packet.Span);
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
                    // Use dedicated buffer for each packet to avoid reuse issues
                    var packetBuffer = new byte[1024]; // Login packets are typically small
                    var count = net.Socket.Receive(packetBuffer);
                    
                    if (count == 0)
                    {
                        FConsole.WriteLine($"[LOGIN] Client disconnected gracefully");
                        break;
                    }
                    
                    // Validate packet size
                    if (count > 1024 || count < 4)
                    {
                        FConsole.WriteLine($"[LOGIN] Invalid packet size {count}, disconnecting client");
                        break;
                    }
                    
                    // Create packet memory with exact size
                    Memory<byte> packet = new byte[count];
                    packetBuffer.AsSpan(0, count).CopyTo(packet.Span);
                    
                    // Decrypt in-place on the dedicated packet buffer
                    net.AuthCrypto.Decrypt(packet.Span, packet.Span);
                    
                    LoginPacketHandler.Process(in player, in packet);
                }
            }
            catch (Exception ex)
            {
                FConsole.WriteLine($"[LOGIN] Client error: {ex.Message}");
            }
            finally
            {
                net.Socket?.Close();
                net.Socket?.Dispose();
            }
        }
    }
}