using MagnumOpus.ECS;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Networking
{
    public static class NetworkHelper
    {
        public static void FullSync(in PixelEntity to, PixelEntity ntt)
        {
            Memory<byte> spawnPacket = MsgSpawn.Create(ntt);
            to.NetSync(in spawnPacket);
        }
        public static void Broadcast(in Memory<byte> packet)
        {
            foreach (PixelEntity other in PixelWorld.Players)
                other.NetSync(in packet);
        }
    }
}