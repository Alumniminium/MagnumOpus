using MagnumOpus.Components.NetSyncComponents;
using MagnumOpus.ECS;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class NetSyncSystem : PixelSystem<DirtySyncComponent>
    {
        public NetSyncSystem() : base("Net Sync System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref DirtySyncComponent sync)
        {
            
            ntt.Remove<DirtySyncComponent>();
        }
    }
}