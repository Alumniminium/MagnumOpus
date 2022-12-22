using MagnumOpus.ECS;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Networking
{
    public static class NetworkHelper
    {
        public static void FullSync(in PixelEntity to, in PixelEntity ntt)
        {
            if (ntt.Type == EntityType.Npc)
            {
                Memory<byte> spawnPacket = MsgNpcSpawn.Create(ntt);
                to.NetSync(in spawnPacket);
            }
            else
            {
                Memory<byte> spawnPacket = MsgSpawn.Create(ntt);
                to.NetSync(in spawnPacket);
            }
        }
        public static void Broadcast(in Memory<byte> packet)
        {
            foreach (PixelEntity other in PixelWorld.Players)
                other.NetSync(in packet);
        }
    }
}