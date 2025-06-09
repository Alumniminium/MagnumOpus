using System.Reflection;
using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking;
using NttECS.ECS;

namespace MagnumOpus.Systems;

public class PacketsIn : NttSystem<NetworkComponent>
{
    public static readonly Dictionary<PacketId, Action<NTT, Memory<byte>>> PacketHandlers = [];

    public PacketsIn() : base("PacketsIn", threads: 1, log: false)
    {
        // === DISCOVER AND REGISTER PACKET HANDLERS ===
        // Use reflection to find all methods with PacketHandlerAttribute
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public))
            {
                var attributes = method.GetCustomAttributes(typeof(PacketHandlerAttribute), false);
                if (attributes.Length == 0)
                    continue;

                // Register packet handler with its packet ID
                var attribute = (PacketHandlerAttribute)attributes[0];
                var handler = (Action<NTT, Memory<byte>>)Delegate.CreateDelegate(typeof(Action<NTT, Memory<byte>>), method);
                PacketHandlers.Add(attribute.Id, handler);
            }
        }
    }

    // Processes incoming network packets by routing them to appropriate handler methods based
    // on packet type. The system uses reflection during initialization to automatically discover
    // and register packet handlers with PacketHandlerAttribute. Each frame, it dequeues packets
    // from entity network queues and invokes the corresponding handler method.
    public override void Update(in NTT ntt, ref NetworkComponent networkComponent)
    {
        // === PROCESS QUEUED PACKETS ===
        // Check each packet type queue for incoming packets
        foreach (var packetQueue in networkComponent.PacketQueues)
        {
            if (!packetQueue.Value.TryDequeue(out var packetData))
                continue;

            if (IsLogging)
                FConsole.WriteLine("[{tick}] Processing {packet} from {ntt}", NttWorld.Tick, packetQueue.Key, ntt);

            // Route packet to registered handler
            if (PacketHandlers.TryGetValue(packetQueue.Key, out var handler))
            {
                handler.Invoke(ntt, packetData);
                return;
            }

            // Log unknown packet types for debugging
            FConsole.WriteLine("[GAME] Unknown packet {packet} ({id}) from {ntt}, no handler registered", packetQueue.Key, (int)packetQueue.Key, ntt.Id);
            FConsole.WriteLine(packetData.Dump());
        }
    }
}
