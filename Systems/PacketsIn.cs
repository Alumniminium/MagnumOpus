using System.Reflection;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking;

namespace MagnumOpus.Systems
{
    public class PacketsIn : NttSystem<NetworkComponent>
    {
        public static readonly Dictionary<PacketId, Action<NTT, Memory<byte>>> PacketHandlers = new();

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

        public override void Update(in NTT ntt, ref NetworkComponent net)
        {
            foreach (var kvp in net.PacketQueues)
            {
                if (kvp.Value.TryDequeue(out var packet))
                {
                    if (IsLogging)
                        Logger.Debug("[{tick}] Processing {packet} from {ntt}", NttWorld.Tick, kvp.Key, ntt);

                    PacketHandlers[kvp.Key].Invoke(ntt, packet);
                }
            }
        }
    }
}
