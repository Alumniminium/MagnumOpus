using MagnumOpus.ECS;
using MagnumOpus.Simulation.Components;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Networking
{
    public static class NetworkHelper
    {
        public static void FullSync(PixelEntity to, PixelEntity ntt)
        {
            ref readonly var phy = ref ntt.Get<BodyComponent>();
            Memory<byte> spawnPacket = MsgSpawn.Create(ntt);
            to.NetSync(spawnPacket);
        }
        public static void Broadcast(Memory<byte> packet)
        {
            foreach (PixelEntity other in PixelWorld.Players)
                other.NetSync(packet);
        }
    }
}