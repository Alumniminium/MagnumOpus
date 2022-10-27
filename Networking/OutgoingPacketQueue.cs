using System.Collections.Concurrent;
using MagnumOpus.ECS;
using MagnumOpus.Simulation.Components;
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
            // lock (SyncRoot)
            queue.Enqueue(packet);
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
                            // try
                            // {
                            //     var bigPacketIndex = 0;
                            //     var bigPacket = new Memory<byte>(new byte[MAX_PACKET_SIZE]);

                            //     while (queue.Count != 0 && bigPacketIndex + MemoryMarshal.Read<ushort>(queue.Peek().Span) < MAX_PACKET_SIZE)
                            //     {
                            //         try
                            //         {
                            //             var packet = queue.Dequeue();
                            //             var size = MemoryMarshal.Read<ushort>(packet.Span);
                            //             var id = MemoryMarshal.Read<ushort>(packet.Span[2..]);
                            //             if(size != packet.Length)
                            //                 Debugger.Break();
                            //             packet.Span[..size].CopyTo(bigPacket.Span[bigPacketIndex..]);
                            //             bigPacketIndex += size;
                            //         }
                            //         catch (Exception e)
                            //         {
                            //             FConsole.WriteLine(e);
                            //         }
                            //     }

                            //     try
                            //     {
                            //         await net.Socket.SendAsync(bigPacket[..bigPacketIndex]).ConfigureAwait(false);
                            //     }
                            //     catch (Exception e)
                            //     {
                            //         if(ntt.Has<PhysicsComponent>())
                            //         {
                            //             var bdy = ntt.Get<PhysicsComponent>();
                            //             Game.Grids[(int)pos.Position.Z].Remove(ntt);
                            //         }
                            //         PixelWorld.Destroy(in ntt);
                            //         FConsole.WriteLine(e.Message);
                            //     }

                            //     if (!net.Socket.Connected)
                            //         break;
                            // }
                            // catch (Exception e)
                            // {
                            //     FConsole.WriteLine(e.Message);
                            // }
                        }
                    }
                    catch (Exception e)
                    {
                        FConsole.WriteLine(e.Message);
                        queue.Clear();
                        ConquerWorld.Players.Remove(ntt);
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
