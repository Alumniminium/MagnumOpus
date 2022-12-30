using MagnumOpus.ECS;

namespace MagnumOpus.Networking
{
    public static class IncomingPacketQueue
    {
        private static readonly Dictionary<PixelEntity, Queue<Memory<byte>>> Queues = new();
        public static void Add(in PixelEntity player, in Memory<byte> packet)
        {
            if (!Queues.TryGetValue(player, out var queue))
            {
                queue = new Queue<Memory<byte>>();
                Queues.Add(player, queue);
            }
            if (packet.IsEmpty)
                return;
            queue.Enqueue(packet);
        }

        public static void Remove(in PixelEntity player) => Queues.Remove(player);

        public static void ProcessAll()
        {
            foreach (var (ntt, queue) in Queues)
            {
                while (queue.Count > 0)
                {
                    var packet = queue.Dequeue();
                    GamePacketHandler.Process(in ntt, in packet);
                }
            }
        }
    }
}