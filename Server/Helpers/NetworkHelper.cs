
using MagnumOpus.Components;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;
using NttECS.ECS;

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
        public static void FullSync(in NTT to, in NTT ntt, bool showEffect = false)
        {
            if (!to.IsPlayer())
                return;
            if (ntt.IsNpc())
            {
                var spawnPacket = MsgNpcSpawn.Create(ntt);
                to.NetSync(ref spawnPacket);
            }
            else if (ntt.IsPlayer() || ntt.IsMonster(guardsAreMonsters: true))
            {
                var spawnPacket = MsgSpawn.Create(ntt);
                to.NetSync(ref spawnPacket);
            }
            else if (ntt.IsItem())
            {
                var spawnPacket = MsgFloorItem.Create(in ntt, MsgFloorItemType.Create);
                to.NetSync(ref spawnPacket);
            }
            else
            {
                if (ntt.Has<BodyComponent>())
                {
                    var spawnPacket = MsgSpawn.CreatePlayer(ntt);
                    to.NetSync(ref spawnPacket);
                }
            }
            if (showEffect)
            {
                var spawnEffectMsg = MsgName.Create(ntt.Id, "MBStandard", MsgNameType.RoleEffect);
                to.NetSync(ref spawnEffectMsg);
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
            if (!to.IsPlayer())
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

        /// <summary>
        /// Updates a network-synced field and automatically handles ChangedTick and network sync.
        /// Used by NetworkSync properties to provide transparent network synchronization.
        /// </summary>
        public static void UpdateSyncedField<TComponent, TValue>(
            ref TComponent component,
            ref TValue field,
            TValue newValue,
            MsgUserAttribType msgType,
            in NTT ntt)
            where TComponent : struct
        {
            // Check if value actually changed
            if (EqualityComparer<TValue>.Default.Equals(field, newValue))
                return;

            // Update field value
            field = newValue;

            // Send network packet if entity is valid
            if (ntt.Id != 0)
            {
                SendNetworkPacket(ntt, msgType, newValue);
            }
        }

        /// <summary>
        /// Sends a network packet to notify clients of a field change.
        /// Creates and broadcasts a MsgUserAttrib packet with the new value.
        /// </summary>
        /// <typeparam name="TValue">The type of value being synchronized</typeparam>
        /// <param name="ntt">The entity whose field changed</param>
        /// <param name="msgType">The attribute type for network packet routing</param>
        /// <param name="value">The new field value to broadcast</param>
        private static void SendNetworkPacket<TValue>(in NTT ntt, MsgUserAttribType msgType, TValue value)
        {
            var packet = MsgUserAttrib.Create(ntt.Id, ConvertToUInt(value), msgType);
            ntt.NetSync(ref packet, true);
        }

        /// <summary>
        /// Converts various value types to uint for network packet transmission.
        /// Supports common numeric types, booleans, and enums with safe casting.
        /// </summary>
        /// <typeparam name="T">The type of value to convert</typeparam>
        /// <param name="value">The value to convert to uint</param>
        /// <returns>The value as a uint for network transmission</returns>
        /// <exception cref="NotSupportedException">Thrown when the type cannot be converted to uint</exception>
        /// <example>
        /// // Convert various types for network packets
        /// uint healthValue = ConvertToUInt(150);        // int to uint
        /// uint boolValue = ConvertToUInt(true);         // bool to uint (1)
        /// uint enumValue = ConvertToUInt(Direction.North); // enum to uint
        /// </example>
        private static uint ConvertToUInt<T>(T value)
        {
            return value switch
            {
                uint ui => ui,
                int i => (uint)i,
                ushort us => us,
                short s => (ushort)s,
                byte b => b,
                sbyte sb => (byte)sb,
                bool bl => bl ? 1u : 0u,
                Enum e => Convert.ToUInt32(e),
                _ => throw new NotSupportedException($"Cannot convert {typeof(T)} to uint for network packet")
            };
        }
    }
}