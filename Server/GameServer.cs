using System.Net.Sockets;
using System.Text;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
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

                    // Use dedicated buffer for DH key exchange packet
                    var dhBuffer = new byte[1024];
                    var count = net.Socket.Receive(dhBuffer);
                    
                    if (count == 0 || count > 1024)
                    {
                        FConsole.WriteLine($"[GAME] Invalid DH packet size {count}, disconnecting");
                        net.Socket?.Close();
                        NttWorld.Destroy(ntt);
                        continue;
                    }
                    
                    Memory<byte> packet = new byte[count];
                    dhBuffer.AsSpan(0, count).CopyTo(packet.Span);
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
            var crypto = net.GameCrypto;

            try
            {
                while (true)
                {
                    // Read packet size (first 2 bytes) into dedicated buffer
                    var sizeBuffer = new byte[2];
                    var received = 0;
                    
                    while (received < 2)
                    {
                        var count = net.Socket.Receive(sizeBuffer.AsSpan(received));
                        if (count == 0)
                        {
                            FConsole.WriteLine($"[GAME] Client disconnected during size read: {net.Username}");
                            return;
                        }
                        received += count;
                    }

                    crypto.Decrypt(sizeBuffer);
                    var size = BitConverter.ToUInt16(sizeBuffer) + 8;

                    // Validate packet size to prevent buffer overflow
                    if (size > 4096 || size < 8)
                    {
                        FConsole.WriteLine($"[GAME] Invalid packet size {size} from {net.Username}, disconnecting");
                        return;
                    }

                    // Allocate dedicated buffer for this packet (eliminates race condition)
                    var packetBuffer = new byte[size];
                    
                    // Copy size bytes to start of packet buffer
                    sizeBuffer.CopyTo(packetBuffer.AsSpan(0, 2));
                    
                    // Read remaining packet data into dedicated buffer
                    received = 2;
                    while (received < size)
                    {
                        var count = net.Socket.Receive(packetBuffer.AsSpan(received));
                        if (count == 0)
                        {
                            FConsole.WriteLine($"[GAME] Client disconnected during packet read: {net.Username}");
                            return;
                        }
                        received += count;
                    }

                    // Decrypt payload (skip first 2 bytes which are already decrypted)
                    crypto.Decrypt(packetBuffer.AsSpan(2));

                    // Extract packet ID and queue packet
                    var id = (PacketId)BitConverter.ToUInt16(packetBuffer.AsSpan(2, 2));
                    
                    // Create final packet copy for queue (ensures thread safety)
                    Memory<byte> finalPacket = new byte[size];
                    packetBuffer.CopyTo(finalPacket);
                    
                    net.PacketQueues[id].Enqueue(finalPacket);
                }
            }
            catch (Exception ex)
            {
                FConsole.WriteLine($"[GAME] Client error for {net.Username}: {ex.Message}");
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