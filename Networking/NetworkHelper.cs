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
                var spawnPacket = MsgNpcSpawn.Create(ntt);
                to.NetSync(ref spawnPacket);
            }
            else
            {
                var spawnPacket = MsgSpawn.Create(ntt);
                to.NetSync(ref spawnPacket);
            }
        }
    }
}