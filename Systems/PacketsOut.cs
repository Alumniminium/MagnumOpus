using System.Net.Sockets;
using System.Text;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Networking
{
    public class PacketsOut : NttSystem<NetworkComponent>
    {
        public static readonly Memory<byte> TqServer = Encoding.ASCII.GetBytes("TQServer");
        public PacketsOut() : base("Packets Out", threads: 12) { }

        // private const int MAX_PACKET_SIZE = 1024 * 8;
        public override void Update(in NTT ntt, ref NetworkComponent net)
        {
            try
            {
                while (net.SendQueue.TryDequeue(out var packet))
                {
                    if(packet.Length < 4)
                        continue;

                    // var id = BitConverter.ToInt16(packet.Span[2..4]);
                    // FConsole.WriteLine($"Sending {(PacketId)id} {id} (Size: {packet.Length}) to {ntt.Id}...");

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

                    net.Socket.Send(packet.Span);
                    // net.Socket.Send(pac-ket.Span);
                }
            }
            catch
            {
                NttWorld.Destroy(in ntt);
            }
        }
    }
}