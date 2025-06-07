using System.Collections.Concurrent;
using System.Net.Sockets;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Cryptography;
using NttECS.ECS;
namespace MagnumOpus.Components;

[Component]
/// <summary>
/// Core networking component that manages client connection state and packet processing.
/// Contains socket connection, cryptography for secure communication, packet queues organized
/// by type, send queue for outgoing data, and username for identification. Not saved to database
/// (no SaveEnabled). Used by PacketsOut system for network transmission and extension methods
/// for player identification. Essential for all client-server communication.
/// </summary>
public struct NetworkComponent
{
    public Socket Socket;
    public Crypto Crypto = new();
    public Dictionary<PacketId, ConcurrentQueue<Memory<byte>>> PacketQueues = [];
    public ConcurrentQueue<byte[]> SendQueue = new();
    public string Username;

    public NetworkComponent(Socket socket)
    {
        Socket = socket;
        Username = string.Empty;

        var packetIds = Enum.GetValues<PacketId>();
        foreach (var packetId in packetIds)
            PacketQueues[packetId] = new ConcurrentQueue<Memory<byte>>();
    }
}