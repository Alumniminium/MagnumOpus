using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Networking
{
    public static class NetworkHelper
    {
        public static void FullSync(in NTT to, in NTT ntt)
        {
            if(to.Type != EntityType.Player)
                return;
            if (ntt.Type == EntityType.Npc)
            {
                var spawnPacket = MsgNpcSpawn.Create(ntt);
                to.NetSync(ref spawnPacket);
            }
            else if (ntt.Type == EntityType.Player || ntt.Type == EntityType.Monster)
            {
                var spawnPacket = MsgSpawn.Create(ntt);
                to.NetSync(ref spawnPacket);
            }
            else if (ntt.Type == EntityType.Item)
            {
                var spawnPacket = MsgFloorItem.Create(in ntt, Enums.MsgFloorItemType.Create);
                to.NetSync(ref spawnPacket);
            }
        }

        internal static void SendMsgTo(in NTT to, string text, MsgTextType channel)
        {
            if(to.Type != EntityType.Player)
                return;
            var msgText = MsgText.Create(in to, text, channel);
            to.NetSync(ref msgText);
        }
        internal static void BroadcastMsg(string text, MsgTextType channel, string from = "SYSTEM")
        {
            var msgText = MsgText.Create(from, "ALLUSERS", text, channel);

            foreach (var ntt in NttWorld.Players)
                ntt.NetSync(ref msgText);
        }
    }
}