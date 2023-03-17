using System.Text;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;

namespace MagnumOpus.Systems
{
    public class PacketsOut : NttSystem<NetworkComponent>
    {
        public static readonly Memory<byte> TqServer = Encoding.ASCII.GetBytes("TQServer");
        public PacketsOut() : base("Packets Out", threads: 2) { }
        public override void Update(in NTT ntt, ref NetworkComponent net)
        {
            try
            {
                while (net.SendQueue.TryDequeue(out var packet))
                {
                    if (packet.Length < 4)
                        continue;

                    if (IsLogging)
                    {
                        var id = BitConverter.ToInt16(packet.Span[2..4]);
                        Logger.Debug(packet.Dump());
                        Logger.Debug("Sending {id}/{id} (Size: {Length}) to {ntt}...", ((PacketId)id).ToString(), id, packet.Length, ntt);
                    }
                    if (net.UseGameCrypto)
                    {
                        Memory<byte> resized = new byte[packet.Length + 8];
                        packet.CopyTo(resized);
                        TqServer.CopyTo(resized[^8..]);
                        net.GameCrypto.Encrypt(resized.Span);
                        packet = resized;
                    }
                    else
                        net.AuthCrypto.Encrypt(packet.Span);

                    _ = net.Socket.Send(packet.Span);
                }
            }
            catch
            {
                NttWorld.Destroy(ntt);
            }
        }
    }
}