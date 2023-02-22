using MagnumOpus.Components;
using MagnumOpus.ECS;

namespace MagnumOpus.Networking
{
    public class PacketsIn : NttSystem<NetworkComponent>
    {
        public PacketsIn() : base("PacketsIn", threads: 1) { }

        public override void Update(in NTT ntt, ref NetworkComponent net)
        {
            while (net.RecvQueue.TryDequeue(out var packet))
                GamePacketHandler.Process(in ntt, in packet);
        }
    }
}