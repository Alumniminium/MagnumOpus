using MagnumOpus.ECS;

namespace MagnumOpus.Networking
{
    public static class IncomingPacketQueue
    {
        private static readonly Dictionary<PixelEntity, Queue<Memory<byte>>> _next = new();
        private static readonly Dictionary<PixelEntity, Queue<Memory<byte>>> Packets = new();
        private static bool processing = false;
        public static void Add(in PixelEntity player, in Memory<byte> packet)
        {
            // if(processing)
            // {
            //     if (!_next.TryGetValue(player, out var queue))
            //     {
            //         queue = new Queue<Memory<byte>>();
            //         _next.Add(player, queue);
            //     }
            //     if (packet.IsEmpty)
            //         return;
            //     queue.Enqueue(packet);
            // }
            // else
            // {
                if (!Packets.TryGetValue(player, out var queue))
                {
                    queue = new Queue<Memory<byte>>();
                    Packets.Add(player, queue);
                }
                if (packet.IsEmpty)
                    return;
                queue.Enqueue(packet);
            // }
        }

        public static void Remove(in PixelEntity player) => Packets.Remove(player);

        public static void ProcessAll()
        {
            processing = true;
            foreach (var (ntt, queue) in Packets)
            {
                while (queue.Count > 0)
                {
                    var packet = queue.Dequeue();
                    if (!PixelWorld.EntityExists(in ntt))
                    {
                        queue.Clear();
                        continue;
                    }                    
                    GamePacketHandler.Process(in ntt, in packet);
                }
                // if(_next.TryGetValue(ntt, out var nextQueue))
                // {
                //     while(nextQueue.Count >0)
                //         Packets[ntt].Enqueue(_next[ntt].Dequeue());
                // }
            }
            processing = false;
        }
    }
}