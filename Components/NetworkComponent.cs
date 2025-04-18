using System.Collections.Concurrent;
using System.Net.Sockets;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Cryptography;
namespace MagnumOpus.Components
{
    [Component]
    public struct NetworkComponent
    {
        public Socket Socket;
        public bool UseGameCrypto;
        public Memory<byte> RecvBuffer;
        public TQCipher AuthCrypto = new();
        public BlowfishCipher GameCrypto = new();
        public DiffieHellman DiffieHellman = new();
        public Memory<byte> ClientIV = new byte[8];
        public Memory<byte> ServerIV = new byte[8];
        public Dictionary<PacketId, ConcurrentQueue<Memory<byte>>> PacketQueues = [];
        public ConcurrentQueue<byte[]> SendQueue = new();
        public string Username;


        public NetworkComponent(Socket socket, byte[]? civ = null, byte[]? siv = null)
        {
            UseGameCrypto = false;
            Socket = socket;
            RecvBuffer = new byte[4096];
            ClientIV = civ ?? new byte[8];
            ServerIV = siv ?? new byte[8];
            Username = string.Empty;
            Random.Shared.NextBytes(ClientIV.Span);
            Random.Shared.NextBytes(ServerIV.Span);

            var packetIds = Enum.GetValues(typeof(PacketId));
            foreach (PacketId packetId in packetIds)
                PacketQueues[packetId] = new ConcurrentQueue<Memory<byte>>();
        }
    }
}