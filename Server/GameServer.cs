using System.Net.Sockets;
using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using NttECS.ECS;

namespace MagnumOpus
{
    public static class GameServer
    {
        private static readonly TcpListener listener = new(System.Net.IPAddress.Any, Constants.GamePort); 
        private static readonly Thread thread = new(GameServerLoop) { IsBackground = true, Priority = ThreadPriority.Highest };

        public static void Start()
        {
            FConsole.WriteLine($"[GAME] Listening on port {Constants.GamePort}...");
            listener.Start();
            thread.Start();
        }

        private static void GameServerLoop()
        {
            while (true)
            {
                var client = listener.AcceptTcpClient();
                FConsole.WriteLine($"[GAME] Client connected: {client.Client.RemoteEndPoint}");
                var ipendpoint = client.Client.RemoteEndPoint?.ToString();

                if (ipendpoint == null) break;
                var (found, ntt) = IpRegistry.GetEntity(ipendpoint.Split(':')[0]);
                if (!found) continue;

                ref var net = ref ntt.Get<NetworkComponent>();
                net.Socket.Close();
                net.Socket.Dispose();
                net.Socket = client.Client;

                try
                {
                    new Thread(() => GameClientLoop(ntt)).Start();
                }
                catch (Exception e)
                {
                    FConsole.WriteLine(e.Message);
                    NttWorld.Destroy(ntt);
                }
            }
        }

        private static void GameClientLoop(NTT ntt)
        {
            ref var net = ref ntt.Get<NetworkComponent>();
            var buffer = new Memory<byte>(new byte[1024]);
            var crypto = net.Crypto;
            net.Crypto.ResetCounters();

            try
            {
                while (true)
                {
                    var sizeBytes = buffer;
                    var count = net.Socket.Receive(sizeBytes.Span);

                    if (count == 0)
                        break;

                    crypto.Decrypt(sizeBytes.Span, count);
                    var size = BitConverter.ToUInt16(sizeBytes.Span[..2]);

                    while (count < size)
                    {
                        var received = net.Socket.Receive(buffer.Span[count..size]);
                        if (received == 0)
                            throw new SocketException((int)SocketError.Disconnecting);
                        count += received;
                        crypto.Decrypt(buffer.Span[count..size], received);
                    }
                    Memory<byte> copy = new byte[size];
                    buffer[..size].CopyTo(copy);

                    var id = (PacketId)BitConverter.ToUInt16(copy.Span[2..4]);
                    net.PacketQueues[id].Enqueue(copy);
                }
            }
            catch
            {
                FConsole.WriteLine($"[GAME] Client disconnected: {net.Username}");
            }
            finally
            {
                net.Socket?.Close();
                net.Socket?.Dispose();
                ntt.Remove<NetworkComponent>();
                NetworkHelper.Despawn(ntt);
            }
        }
    }
}