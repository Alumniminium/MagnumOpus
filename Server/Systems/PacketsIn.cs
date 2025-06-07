using System.Reflection;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking;

namespace MagnumOpus.Systems
{
    /// <summary>
    /// Processes incoming network packets by routing them to appropriate handler methods based on packet type.
    /// Uses reflection to automatically discover and register packet handlers with PacketHandlerAttribute.
    /// </summary>
    public class PacketsIn : NttSystem<NetworkComponent>
    {
        public static readonly Dictionary<PacketId, Action<NTT, Memory<byte>>> PacketHandlers = new();

        /// <summary>
        /// Initializes PacketsIn system with single-threaded processing and discovers packet handlers via reflection.
        /// </summary>
        public PacketsIn() : base("PacketsIn", threads: 1, log: false)
        {
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public))
                {
                    var attributes = method.GetCustomAttributes(typeof(PacketHandlerAttribute), false);
                    if (attributes.Length == 0)
                        continue;

                    var attribute = (PacketHandlerAttribute)attributes[0];
                    PacketHandlers.Add(attribute.Id, (Action<NTT, Memory<byte>>)Delegate.CreateDelegate(typeof(Action<NTT, Memory<byte>>), method));
                }
            }
        }

        /// <summary>
        /// Processes queued incoming packets for an entity by invoking appropriate packet handlers.
        /// </summary>
        /// <param name="ntt">The entity receiving packets</param>
        /// <param name="networkComponent">Network component containing packet queues</param>
        public override void Update(in NTT ntt, ref NetworkComponent networkComponent)
        {
            foreach (var packetQueue in networkComponent.PacketQueues)
            {
                if (packetQueue.Value.TryDequeue(out var packetData))
                {
                    if (IsLogging)
                        FConsole.WriteLine("[{tick}] Processing {packet} from {ntt}", NttWorld.Tick, packetQueue.Key, ntt);

                    PacketHandlers[packetQueue.Key].Invoke(ntt, packetData);
                }
            }
        }
    }
}
