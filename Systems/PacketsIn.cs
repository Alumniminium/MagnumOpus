using System.Reflection;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;

namespace MagnumOpus.Networking
{
    public class PacketsIn : NttSystem<NetworkComponent>
    {
        public static readonly Dictionary<PacketId, Action<NTT, Memory<byte>>> PacketHandlers = new();

        public PacketsIn() : base("PacketsIn", threads: 1)
        {
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
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

        public override void Update(in NTT ntt, ref NetworkComponent net)
        {
            try
            {
            while (net.RecvQueue.TryDequeue(out var packet))
            {
                var packetType = (PacketId)BitConverter.ToUInt16(packet.Span[2..4]);
                if (Trace) 
                    Logger.Debug("Received {packet} from {ntt}", packetType, ntt);

                if (PacketHandlers.TryGetValue(packetType, out var handler))
                    handler.Invoke(ntt, packet);
                else if (Trace)
                    Logger.Debug("Undefined PacketId: {packet} {dump}", packetType, packet.Dump());
            }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in PacketsIn");
            }
        }
    }
}