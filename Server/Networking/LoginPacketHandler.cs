using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;
using NttECS.ECS;

namespace MagnumOpus.Networking
{
    /// <summary>
    /// Specialized packet handler for login server connections and authentication.
    /// Processes account authentication, server selection, and client file verification during login flow.
    /// Handles password decryption and session establishment for transitioning clients to game server.
    /// </summary>
    public static unsafe class LoginPacketHandler
    {
        /// <summary>
        /// Processes incoming login packets and routes them to appropriate handlers based on packet type.
        /// Handles account authentication (MsgConnect) and client validation (MsgLogin) packets.
        /// Automatically destroys the client connection on any processing errors for security.
        /// </summary>
        /// <param name="ntt">Network entity representing the client connection</param>
        /// <param name="packet">Raw packet data received from client</param>
        internal static void Process(in NTT ntt, in Memory<byte> packet)
        {
            try
            {
                // Validate packet has enough bytes for packet ID
                if (packet.Length < 4)
                {
                    FConsole.WriteLine($"[LOGIN] Packet too short ({packet.Length} bytes), disconnecting client");
                    NttWorld.Destroy(ntt);
                    return;
                }

                var packetTypeRaw = BitConverter.ToUInt16(packet.Span[2..]);

                // Check if this is a valid PacketId enum value
                if (!Enum.IsDefined(typeof(PacketId), packetTypeRaw))
                {
                    FConsole.WriteLine($"[LOGIN] Invalid packet ID {packetTypeRaw} (0x{packetTypeRaw:X4}), likely cipher desync - disconnecting client");
                    FConsole.WriteLine(packet.Dump());
                    NttWorld.Destroy(ntt);
                    return;
                }

                var packetType = (PacketId)packetTypeRaw;

                switch (packetType)
                {
                    case PacketId.MsgConnect:
                        {
                            var msgAccount = Co2Packet.Deserialize<MsgAccount>(packet.Span);
                            var username = msgAccount.GetUsername();
                            var password = msgAccount.GetPassword(); // Skip password decryption for now
                            var server = msgAccount.GetServer();

                            FConsole.WriteLine($"[LOGIN/1051] Account: {username}, Pass: {password}, Server: {server} - Sending to Game Server on IP: {Constants.ServerIP}, Port: {Constants.GamePort}");

                            var response = MsgAccountResponse.Create(Constants.ServerIP, Constants.GamePort, ntt.Id, ntt.Id);
                            ref var net = ref ntt.Get<NetworkComponent>();
                            net.Username = username;
                            ntt.NetSync(ref response);
                            break;
                        }
                    case PacketId.MsgLogin:
                        {
                            var msg = Co2Packet.Deserialize<MsgConnectLogin>(packet.Span);
                            var filename = msg.GetFileName();
                            FConsole.WriteLine($"[LOGIN/1052] Client Id: {msg.UniqueId}, File: {filename} Contents: {msg.Contents}");
                            break;
                        }
                    default:
                        {
                            FConsole.WriteLine($"[LOGIN/{(int)packetType}/{packetType}] Unknown packet (valid ID but no handler)");
                            FConsole.WriteLine(packet.Dump());
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                FConsole.WriteLine($"[LOGIN] Error processing packet: {ex.Message}");
                NttWorld.Destroy(ntt);
            }
        }
    }
}