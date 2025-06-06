using MagnumOpus.Enums;

namespace MagnumOpus.Networking
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PacketHandlerAttribute(PacketId id) : Attribute
    {
        public PacketId Id { get; } = id;
    }
}