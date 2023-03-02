using MagnumOpus.Enums;

namespace MagnumOpus.Networking
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PacketHandlerAttribute : Attribute
    {
        public PacketId Id { get; }
        public PacketHandlerAttribute(PacketId id) => Id = id;
    }   
}