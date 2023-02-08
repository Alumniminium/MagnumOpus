using System.Reflection;
using HerstLib.IO;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;

namespace MagnumOpus.Networking
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PacketHandlerAttribute : Attribute
    {
        public PacketId Id { get; }
        public PacketHandlerAttribute(PacketId id) => Id = id;
    }
    public static unsafe class GamePacketHandler
    {
        public static readonly Dictionary<PacketId, Action<NTT, Memory<byte>>> PacketHandlers = new();

        static GamePacketHandler()
        {
            foreach(var type in  Assembly.GetExecutingAssembly().GetTypes())
            {
                var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(PacketHandlerAttribute), false);
                    if (attributes.Length == 0)
                        continue;
                    var attribute = (PacketHandlerAttribute)attributes[0];
                    PacketHandlers.Add(attribute.Id, (Action<NTT, Memory<byte>>)Delegate.CreateDelegate(typeof(Action<NTT, Memory<byte>>), method));
                }
            }
        }

        public static void Process(in NTT player, in Memory<byte> packet)
        {
            var packetType = (PacketId)BitConverter.ToUInt16(packet.Span[2..4]);

            if(PacketHandlers.TryGetValue(packetType, out var handler))
                handler.Invoke(player, packet);
            else
            {
                FConsole.WriteLine("Undefined PacketId: " + BitConverter.ToInt16(packet.Span[2..4]));
                FConsole.WriteLine(packet.Dump());
            }
        } 
    }
}