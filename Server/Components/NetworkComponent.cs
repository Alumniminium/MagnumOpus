using System.Collections.Concurrent;
using System.Net.Sockets;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Cryptography;
using NttECS.ECS;
namespace MagnumOpus.Components;

[Component]
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