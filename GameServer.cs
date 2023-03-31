using System.Net.Sockets;
using System.Text;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;

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
                net.UseGameCrypto = true;
                net.Socket.Close();
                net.Socket.Dispose();
                net.Socket = client.Client;

                try
                {
                    net.DiffieHellman.ComputePublicKey();
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
                    for (var i = 0; i < pkSize; i++) pk[i] = packetSpan[19 + junkSize + i];

                    var pubkey = Encoding.ASCII.GetString(pk);
                    net.DiffieHellman.ComputePrivateKey(pubkey);
                    net.GameCrypto.GenerateKeys(net.DiffieHellman.GetPrivateKey());
                    net.GameCrypto.SetIVs(net.ServerIV, net.ClientIV);

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
            var buffer = net.RecvBuffer;
            var crypto = net.GameCrypto;

            try
            {
                while (true)
                {
                    var sizeBytes = buffer[..2];
                    var count = net.Socket.Receive(sizeBytes.Span);
                    if (count != 2)
                        break;

                    crypto.Decrypt(sizeBytes.Span);
                    var size = BitConverter.ToUInt16(sizeBytes.Span) + 8;

                    count = 2;
                    while (count < size)
                    {
                        var received = net.Socket.Receive(buffer.Span[count..size]);
                        if (received == 0)
                            break;
                        count += received;
                    }
                    Memory<byte> copy = new byte[size];
                    buffer[..size].CopyTo(copy);
                    crypto.Decrypt(copy.Span[2..]);
                    net.RecvQueue.Enqueue(copy);
                }
            }
            finally
            {
                FConsole.WriteLine($"[GAME] Client disconnected: {net.Username}");
                net.Socket?.Close();
                net.Socket?.Dispose();
                ntt.Remove<NetworkComponent>();
                NetworkHelper.Despawn(ntt);
            }
        }
    }
}