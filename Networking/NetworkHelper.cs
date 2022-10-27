using MagnumOpus.ECS;
using MagnumOpus.Simulation.Components;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Networking
{
    public static class NetworkHelper
    {
        public static void FullSync(in PixelEntity to, PixelEntity ntt)
        {
            ref readonly var bdy = ref ntt.Get<BodyComponent>();
            Memory<byte> spawnPacket = MsgSpawn.Create(ntt);
            to.NetSync(in spawnPacket);
        }
        public static void Broadcast(in Memory<byte> packet)
        {
            foreach (PixelEntity other in ConquerWorld.Players)
                other.NetSync(in packet);
        }
    }
}