using System.Net.Sockets;
using System.Text;
using MagnumOpus.Components;
using MagnumOpus.ECS;

namespace MagnumOpus.Systems
{
    public class PacketsOut : NttSystem<NetworkComponent>
    {
        public static readonly Memory<byte> TqServer = Encoding.ASCII.GetBytes("TQServer");
        public PacketsOut() : base("Packets Out", threads: 2, log: true) { }
        public override void Update(in NTT ntt, ref NetworkComponent net)
        {
            if (net.SendBufferOffset == 0)
                return;

            try
            {
                if (net.SendBufferOffset == 355) // DiffieHellman key exchange
            {
                var chunkToSend = net.SendBuffer.Slice(0, net.SendBufferOffset);
                    var encryptedChunk = new byte[net.SendBufferOffset + 8];
                    chunkToSend.CopyTo(encryptedChunk);
                    TqServer.CopyTo(encryptedChunk.AsMemory(net.SendBufferOffset..));
                    net.GameCrypto.Encrypt(encryptedChunk);
                    net.Socket.Send(encryptedChunk, SocketFlags.None);

                // Reset the net.SendBufferOffset
                for (var i = 0; i < net.SendBufferOffset; i++)
                    net.SendBuffer.Span[i] = 0;
                net.SendBufferOffset = 0;
                return;
            }

                var maxChunkSize = 850;
                var offset = 0;

                while (offset < net.SendBufferOffset)
                {
                    var remainingBufferSize = net.SendBufferOffset - offset;
                    var chunkSize = 0;

                    while (chunkSize < maxChunkSize && chunkSize < remainingBufferSize)
                    {
                        var packetSize = BitConverter.ToUInt16(net.SendBuffer.Span.Slice(offset + chunkSize, 2));
                        if (packetSize == 0)
                        {
                            break;
                        }

                        chunkSize += packetSize;
                        remainingBufferSize -= packetSize;
                    }

                    if (chunkSize > 0)
                    {
                        var chunkToSend = net.SendBuffer.Slice(offset, chunkSize);
                        if (net.UseGameCrypto)
                        {
                            var encryptedChunk = new byte[chunkSize + 8];
                            chunkToSend.CopyTo(encryptedChunk);
                            TqServer.CopyTo(encryptedChunk.AsMemory(chunkSize..));
                            net.GameCrypto.Encrypt(encryptedChunk);
                            net.Socket.Send(encryptedChunk, SocketFlags.None);
                        }
                        else
                        {
                            net.AuthCrypto.Encrypt(chunkToSend.Span);
                            net.Socket.Send(chunkToSend.Span, SocketFlags.None);
                        }

                        offset += chunkSize;
                    }
                }

                // Reset the net.SendBufferOffset
                for (var i = 0; i < net.SendBufferOffset; i++)
                    net.SendBuffer.Span[i] = 0;
                net.SendBufferOffset = 0;
            }
            catch
            {
                NttWorld.Destroy(ntt);
            }
        }
    }
}