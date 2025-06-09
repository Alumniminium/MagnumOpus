using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using NttECS.ECS;

namespace MagnumOpus.Systems;

public class PacketsOut : NttSystem<NetworkComponent>
{
    public PacketsOut() : base("Packets Out", threads: 1, log: false) { }

    // Handles outgoing network packet transmission with encryption and proper protocol formatting.
    // Manages both authentication and game-level cryptography based on connection state, processes
    // the packet send queue, encrypts data, and sends to clients with error handling for disconnections.
    public override void Update(in NTT ntt, ref NetworkComponent networkComponent)
    {
        try
        {
            // === PROCESS OUTGOING PACKET QUEUE ===
            while (networkComponent.SendQueue.TryDequeue(out var packetBuffer))
            {
                var packetData = packetBuffer.AsSpan();

                // Skip malformed packets
                if (packetData.Length < 4)
                    continue;

                // === LOG PACKET TRANSMISSION ===
                var packetId = BitConverter.ToInt16(packetData[2..4]);
                if (IsLogging)
                {
                    FConsole.WriteLine("Sending {id} (Size: {Length}) to {ntt}...", ((PacketId)packetId).ToString(), packetData.Length, ntt);
                    FConsole.WriteLine(packetData.Dump());
                }

                // === ENCRYPT AND SEND PACKET ===
                networkComponent.Crypto.Encrypt(packetData, packetData.Length);
                networkComponent.Socket.Send(packetData);
            }
        }
        catch
        {
            // Remove network component on transmission failure (client disconnect)
            ntt.Remove<NetworkComponent>();
        }
    }
}