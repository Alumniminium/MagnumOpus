
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Helpers
{
    /// <summary>
    /// Provides network synchronization utilities for entity spawning and messaging.
    /// Handles entity visibility updates and communication between players.
    /// </summary>
    public static class NetworkHelper
    {
        /// <summary>
        /// Synchronizes a target entity's full state to a receiving entity, handling spawn packets and effects.
        /// Only players can receive synchronization data. Different entity types send different spawn packets.
        /// </summary>
        /// <param name="to">The receiving entity (must be a player)</param>
        /// <param name="ntt">The target entity to synchronize</param>
        /// <example>
        /// // Sync a new player to an existing player's viewport
        /// NetworkHelper.FullSync(existingPlayer, newPlayer);
        /// 
        /// // Sync a monster to a player when it enters their view
        /// NetworkHelper.FullSync(player, monster);
        /// </example>
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

                if (ntt.CreatedTick != NttWorld.Tick)
                    return;

                var spawnEffectMsg = MsgName.Create(ntt.Id, "MBStandard", MsgNameType.RoleEffect);
                to.NetSync(ref spawnEffectMsg);
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

        /// <summary>
        /// Sends a text message to a specific player entity.
        /// Non-player entities cannot receive messages and will be ignored.
        /// </summary>
        /// <param name="to">The target player entity to receive the message</param>
        /// <param name="text">The message content to send</param>
        /// <param name="channel">The message channel/type for display formatting</param>
        /// <example>
        /// // Send debug info to a player
        /// NetworkHelper.SendMsgTo(player, "Debug: Position updated", MsgTextType.TopLeft);
        /// 
        /// // Send system message to a player
        /// NetworkHelper.SendMsgTo(player, "Welcome to the server!", MsgTextType.Center);
        /// </example>
        internal static void SendMsgTo(in NTT to, string text, MsgTextType channel)
        {
            if (to.Type != EntityType.Player)
                return;
            var messagePacket = MsgText.Create(in to, text, channel);
            to.NetSync(ref messagePacket);
        }
        
        /// <summary>
        /// Broadcasts a text message to all connected players.
        /// Useful for server-wide announcements and system messages.
        /// </summary>
        /// <param name="text">The message content to broadcast</param>
        /// <param name="channel">The message channel/type for display formatting</param>
        /// <param name="from">The sender name to display (defaults to "SYSTEM")</param>
        /// <example>
        /// // Server maintenance announcement
        /// NetworkHelper.BroadcastMsg("Server restart in 5 minutes", MsgTextType.Center);
        /// 
        /// // Event notification
        /// NetworkHelper.BroadcastMsg("Guild war has started!", MsgTextType.Talk, "EVENT");
        /// </example>
        internal static void BroadcastMsg(string text, MsgTextType channel, string from = "SYSTEM")
        {
            var broadcastPacket = MsgText.Create(from, "ALLUSERS", text, channel);

            foreach (var player in NttWorld.Players)
                player.NetSync(ref broadcastPacket);
        }

        /// <summary>
        /// Removes an entity from all player viewports by sending despawn packets.
        /// Notifies all nearby players that the entity should be removed from their client.
        /// </summary>
        /// <param name="ntt">The entity to despawn from player viewports</param>
        /// <example>
        /// // Remove a monster when it dies
        /// NetworkHelper.Despawn(monsterEntity);
        /// 
        /// // Remove a player when they disconnect
        /// NetworkHelper.Despawn(disconnectedPlayer);
        /// </example>
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