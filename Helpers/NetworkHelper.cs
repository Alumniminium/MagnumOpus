
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Helpers
{
    public static class NetworkHelper
    {
        public static void FullSync(in NTT to, in NTT ntt)
        {
            if (to.Type != EntityType.Player)
                return;
            if (ntt.Type == EntityType.Npc)
            {
                var spawnPacket = MsgNpcSpawn.Create(ntt);
                to.NetSync(ref spawnPacket);
            }
            else if (ntt.Type is EntityType.Player or EntityType.Monster)
            {
                var spawnPacket = MsgSpawn.Create(ntt);
                to.NetSync(ref spawnPacket);

                if (ntt.CreatedTick == NttWorld.Tick)
                {
                    var spawnEffectMsg = MsgName.Create(ntt.Id, "MBStandard", MsgNameType.RoleEffect);
                    to.NetSync(ref spawnEffectMsg);
                }
            }
            else if (ntt.Type == EntityType.Item)
            {
                var spawnPacket = MsgFloorItem.Create(in ntt, MsgFloorItemType.Create);
                to.NetSync(ref spawnPacket);
            }
            else if (ntt.Type == EntityType.Other)
            {
                if (ntt.Has<BodyComponent>())
                {
                    var spawnPacket = MsgSpawn.CreatePlayer(ntt);
                    to.NetSync(ref spawnPacket);
                }
            }
        }

        internal static void SendMsgTo(in NTT to, string text, MsgTextType channel)
        {
            if (to.Type != EntityType.Player)
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

        internal static void Despawn(NTT ntt)
        {
            var despawnPacket = MsgAction.RemoveEntity(ntt.Id);
            ntt.NetSync(ref despawnPacket, true);
        }

        internal static void SyncEquipment(NTT ntt)
        {
            ref readonly var eq = ref ntt.Get<EquipmentComponent>();

            var packets = new MsgItemInformation[9]
            {
                MsgItemInformation.Create(eq.Head, MsgItemInfoAction.AddItem, MsgItemPosition.Head),
                MsgItemInformation.Create(eq.Garment, MsgItemInfoAction.AddItem, MsgItemPosition.Garment),
                MsgItemInformation.Create(eq.Bottle, MsgItemInfoAction.AddItem, MsgItemPosition.Bottle),
                MsgItemInformation.Create(eq.Necklace, MsgItemInfoAction.AddItem, MsgItemPosition.Necklace),
                MsgItemInformation.Create(eq.Ring, MsgItemInfoAction.AddItem, MsgItemPosition.Ring),
                MsgItemInformation.Create(eq.Armor, MsgItemInfoAction.AddItem, MsgItemPosition.Armor),
                MsgItemInformation.Create(eq.RightWeapon, MsgItemInfoAction.AddItem, MsgItemPosition.RightWeapon),
                MsgItemInformation.Create(eq.LeftWeapon, MsgItemInfoAction.AddItem, MsgItemPosition.LeftWeapon),
                MsgItemInformation.Create(eq.Boots, MsgItemInfoAction.AddItem, MsgItemPosition.Boots),
            };

            for (var i = 0; i < packets.Length; i++)
                ntt.NetSync(ref packets[i]);
        }
    }
}