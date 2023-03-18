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

                    var id = BitConverter.ToInt16(packet.Span[2..4]);
                    if (IsLogging)
                    {
                        Logger.Debug(packet.Dump());
                        Logger.Debug("Sending {id}/{id} (Size: {Length}) to {ntt}...", ((PacketId)id).ToString(), id, packet.Length, ntt);
                    }
                    if (net.UseGameCrypto)
                    {
                        Span<byte> resized = stackalloc byte[packet.Length + 8];
                        packet.Span.CopyTo(resized);
                        TqServer.Span.CopyTo(resized[^8..]);
                        net.GameCrypto.Encrypt(resized);
                        net.Socket.Send(resized);
                    }
                    else
                    {
                        net.AuthCrypto.Encrypt(packet.Span);
                        net.Socket.Send(packet.Span);
                    }
                }
            }
            catch
            {
                ntt.Remove<NetworkComponent>();
            }
        }
    }
}