using System.Collections.Concurrent;
using MagnumOpus.ECS;
using MagnumOpus.Components;
using HerstLib.IO;
using System.Net.Sockets;
using System.Text;
using MagnumOpus.Enums;

namespace MagnumOpus.Networking
{
    public static class PacketsOut
    {
        // private const int MAX_PACKET_SIZE = 1024 * 8;
        private static readonly ConcurrentDictionary<NTT, ConcurrentQueue<Memory<byte>>> Packets = new();

        public static void Add(in NTT player, in Memory<byte> packet)
        {
            if (!Packets.TryGetValue(player, out var queue))
            {
                queue = new ConcurrentQueue<Memory<byte>>();
                Packets.TryAdd(player, queue);
            }
            var copy = new byte[packet.Length];
            packet.CopyTo(copy);
            queue.Enqueue(copy);
        }

        public static void Remove(in NTT player) => Packets.TryRemove(player, out _);

        public static void SendAll()
        {
            try
            {
                foreach (var (ntt, queue) in Packets)
                {
                    try
                    {
                        ref var net = ref ntt.Get<NetworkComponent>();
                        if(net.Socket == null || !net.Socket.Connected)
                        {
                            queue.Clear();
                            NttWorld.Players.Remove(ntt);
                            continue;
                        }
                        while (!queue.IsEmpty)
                        {
                            if(!queue.TryDequeue(out var packet))
                                continue;
                            var id = BitConverter.ToInt16(packet.Span[2..4]);
                            // FConsole.WriteLine($"Sending {(PacketId)id} {id} (Size: {packet.Length}) to {ntt.Id}...");
                            if(net.UseGameCrypto)
                            {
                                var resized = new byte[packet.Length + 8];
                                packet.CopyTo(resized);
                                var tqServer = Encoding.ASCII.GetBytes("TQServer");
                                tqServer.CopyTo(resized, resized.Length-8);
                                net.GameCrypto.Encrypt(resized);
                                packet = resized;
                            }
                            else
                                net.AuthCrypto.Encrypt(packet.Span);
                                
                            net.Socket.SendAsync(packet, SocketFlags.None, CancellationToken.None);
                        }
                    }
                    catch (Exception e)
                    {
                        FConsole.WriteLine(e.Message);
                        queue.Clear();
                        NttWorld.Players.Remove(ntt);
                    }
                }
            }
            catch (Exception e)
            {
                FConsole.WriteLine(e.Message);
            }
        }
    }
}
