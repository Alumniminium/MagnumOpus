using System.Text;
using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using NttECS.ECS;

namespace MagnumOpus.Systems
{
    /// <summary>
    /// Handles outgoing network packet transmission with encryption and proper protocol formatting.
    /// Manages both authentication and game-level cryptography based on connection state.
    /// </summary>
    public class PacketsOut : NttSystem<NetworkComponent>
    {
        /// <summary>
        /// Initializes PacketsOut system with limited threading for packet transmission.
        /// </summary>
        public PacketsOut() : base("Packets Out", threads: 1) { }

        /// <summary>
        /// Processes outgoing packet queue, applying appropriate encryption and sending data to clients.
        /// </summary>
        /// <param name="ntt">The entity sending packets</param>
        /// <param name="networkComponent">Network component containing send queue and encryption state</param>
        public override void Update(in NTT ntt, ref NetworkComponent networkComponent)
        {
            try
            {
                while (networkComponent.SendQueue.TryDequeue(out var packetBuffer))
                {
                    var packetData = packetBuffer.AsSpan();

                    if (packetData.Length < 4)
                        continue;

                    var packetId = BitConverter.ToInt16(packetData[2..4]);
                    if (IsLogging)
                    {
                        FConsole.WriteLine(packetData.Dump());
                        FConsole.WriteLine("Sending {id}/{id} (Size: {Length}) to {ntt}...", ((PacketId)packetId).ToString(), packetId, packetData.Length, ntt);
                    }
                    networkComponent.Crypto.Encrypt(packetData, packetData.Length);
                    networkComponent.Socket.Send(packetData);
                }
            }
            catch
            {
                ntt.Remove<NetworkComponent>();
            }
        }
    }
}