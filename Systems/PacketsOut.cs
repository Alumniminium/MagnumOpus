using System.Text;
using HerstLib.IO;
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
                while (net.SendQueue.TryDequeue(out var buffer))
                {
                    var packet = buffer.AsSpan();

                    if (packet.Length < 4)
                        continue;

                    var id = BitConverter.ToInt16(packet[2..4]);
                    if (IsLogging)
                    {
                        FConsole.WriteLine(packet.Dump());
                        FConsole.WriteLine("Sending {id}/{id} (Size: {Length}) to {ntt}...", ((PacketId)id).ToString(), id, packet.Length, ntt);
                    }
                    if (net.UseGameCrypto)
                    {
                        Span<byte> resized = new byte[packet.Length + 8];
                        packet.CopyTo(resized);
                        TqServer.Span.CopyTo(resized[^8..]);
                        net.GameCrypto.Encrypt(resized);
                        net.Socket.Send(resized);
                    }
                    else
                    {
                        net.AuthCrypto.Encrypt(packet);
                        net.Socket.Send(packet);
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