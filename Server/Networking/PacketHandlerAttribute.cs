using MagnumOpus.Enums;

namespace MagnumOpus.Networking
{
    /// <summary>
    /// Attribute for marking methods as packet handlers in the networking system.
    /// Used by reflection-based packet routing to automatically map packet types to handler methods.
    /// Methods decorated with this attribute will be automatically invoked when the specified packet type is received.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class PacketHandlerAttribute(PacketId id) : Attribute
    {
        /// <summary>
        /// The packet identifier that this handler method processes.
        /// When a packet with this ID is received, the decorated method will be invoked.
        /// </summary>
        public PacketId Id { get; } = id;
    }
}