using System.Buffers;
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
                var maxPacketSize = 800 - 8; // Maximum allowed packet size, accounting for the TqServer footer
                var offset = 0;

                while (net.SendQueue.TryDequeue(out var array))
                {
                    var packet = array.AsSpan();
                    if (packet.Length < 4)
                        continue;

                    var id = BitConverter.ToInt16(packet[2..4]);
                    if (IsLogging)
                    {
                        Logger.Debug(array.AsMemory().Dump());
                        Logger.Debug("Sending {id}/{id} (Size: {Length}) to {ntt}...", ((PacketId)id).ToString(), id, packet.Length, ntt);
                    }

                    if (net.UseGameCrypto)
                    {
                        var resized = new byte[packet.Length + 8];
                        packet.CopyTo(resized);
                        ArrayPool<byte>.Shared.Return(array);
                        TqServer.CopyTo(resized.AsMemory()[^8..]);
                        net.GameCrypto.Encrypt(resized);
                        packet = resized;
                    }
                    else
                    {
                        net.AuthCrypto.Encrypt(packet);
                        // Resize the packet to include the TqServer footer
                        var resized = new byte[packet.Length + 8];
                        packet.CopyTo(resized);
                        TqServer.CopyTo(resized.AsMemory()[^8..]);
                        packet = resized;
                    }

                    // Check if the current packet would exceed the maximum allowed size
                    if (offset + packet.Length > maxPacketSize)
                    {
                        // Send the current data in the buffer before adding the new packet
                        net.Socket.Send(net.SendBuffer.AsSpan()[..offset], SocketFlags.None);
                        offset = 0;
                    }

                    packet.CopyTo(net.SendBuffer.AsSpan()[offset..]);
                    offset += packet.Length;
                }

                if (offset > 0)
                {
                    // Send the remaining data in the buffer
                    net.Socket.Send(net.SendBuffer.AsSpan()[..offset], SocketFlags.None);
                }
            }
            catch
            {
                NttWorld.Destroy(ntt);
            }
        }
    }
}