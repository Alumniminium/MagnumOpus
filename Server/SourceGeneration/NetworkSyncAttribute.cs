using System;

namespace MagnumOpus.SourceGeneration
{
    /// <summary>
    /// Marks a field to be auto-generated as a network-synced property.
    /// The generated property will automatically send network updates when the value changes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class NetworkSyncAttribute : Attribute
    {
        /// <summary>
        /// The message type to use for network synchronization.
        /// </summary>
        public object MsgType { get; }

        /// <summary>
        /// Whether to check for value equality before updating (default: true).
        /// </summary>
        public bool UseEqualityCheck { get; set; } = true;

        /// <summary>
        /// Whether to broadcast the update to all clients (default: true).
        /// </summary>
        public bool Broadcast { get; set; } = true;

        /// <summary>
        /// Custom property name (if different from field name).
        /// </summary>
        public string? PropertyName { get; set; }

        public NetworkSyncAttribute(object msgType)
        {
            MsgType = msgType;
        }
    }
}