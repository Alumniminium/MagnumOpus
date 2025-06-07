using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;
using NttECS.ECS;

namespace MagnumOpus.Helpers
{
    /// <summary>
    /// Simple network synchronization helper for property-based NetworkSync system.
    /// Handles ChangedTick updates and network packet sending.
    /// </summary>
    public static class NetworkSyncHelper
    {
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