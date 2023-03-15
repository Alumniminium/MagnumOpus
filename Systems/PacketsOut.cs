using System.Net.Sockets;
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
                int maxPacketSize = 800 - 8; // Maximum allowed packet size, accounting for the TqServer footer
                int offset = 0;

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
                        var resized = new byte[packet.Length + 8];
                        packet.CopyTo(resized);
                        net.GameCrypto.Encrypt(resized);
                        packet = resized;
                    }
                    else
                    {
                        net.AuthCrypto.Encrypt(packet.Span);
                    }

                    // Check if the current packet would exceed the maximum allowed size
                    if (offset + packet.Length > maxPacketSize)
                    {
                        // Send the current data in the buffer before adding the new packet
                        TqServer.CopyTo(net.SendBuffer[offset..]);
                        net.Socket.Send(net.SendBuffer.Span[..(offset + 8)], SocketFlags.None);
                        offset = 0;
                    }

                    packet.Span.CopyTo(net.SendBuffer.Span[offset..]);
                    offset += packet.Length;
                }

                if (offset > 0)
                {
                    // Append the TqServer footer and send the remaining data in the buffer
                    TqServer.CopyTo(net.SendBuffer[offset..]);
                    net.Socket.Send(net.SendBuffer.Span[..(offset + 8)], SocketFlags.None);
                }
            }
            catch
            {
                NttWorld.Destroy(ntt);
            }
        }
    }
}