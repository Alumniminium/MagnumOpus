using System.Collections.Concurrent;
using MagnumOpus.ECS;
using MagnumOpus.Components;
using HerstLib.IO;
using System.Net.Sockets;
using System.Text;

namespace MagnumOpus.Networking
{
    public static class OutgoingPacketQueue
    {
        // private const int MAX_PACKET_SIZE = 1024 * 8;
        private static readonly ConcurrentDictionary<PixelEntity, Queue<Memory<byte>>> Packets = new();

        public static void Add(in PixelEntity player, in Memory<byte> packet)
        {
            if (!Packets.TryGetValue(player, out var queue))
            {
                queue = new Queue<Memory<byte>>();
                Packets.TryAdd(player, queue);
            }
            var copy = new byte[packet.Length];
            packet.CopyTo(copy);
            queue.Enqueue(copy);
        }

        public static void Remove(in PixelEntity player) => Packets.TryRemove(player, out _);

        public static async void SendAll()
        {
            try
            {
                foreach (var (ntt, queue) in Packets)
                {
                    try
                    {
                        var net = ntt.Get<NetworkComponent>();
                        while (queue.Count > 0)
                        {
                            var packet = queue.Dequeue();
                            var id = BitConverter.ToInt16(packet.Span[2..4]);
                            FConsole.WriteLine($"Sending {id} to {ntt.Id}...");
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
                                
                            await net.Socket.SendAsync(packet, SocketFlags.None, CancellationToken.None);
                        }
                    }
                    catch (Exception e)
                    {
                        FConsole.WriteLine(e.Message);
                        queue.Clear();
                        PixelWorld.Players.Remove(ntt);
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
