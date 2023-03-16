using System.Collections.Concurrent;
using System.Net.Sockets;
using MagnumOpus.ECS;
using MagnumOpus.Networking.Cryptography;
namespace MagnumOpus.Components
{
    [Component]
    public struct NetworkComponent
    {
        public Socket Socket;
        public bool UseGameCrypto;
        public byte[] RecvBuffer;
        public byte[] SendBuffer;
        public TQCipher AuthCrypto = new();
        public BlowfishCipher GameCrypto = new();
        public DiffieHellman DiffieHellman = new();
        public Memory<byte> ClientIV = new byte[8];
        public Memory<byte> ServerIV = new byte[8];
        public ConcurrentQueue<byte[]> SendQueue = new();
        public ConcurrentQueue<byte[]> RecvQueue = new();
        public string Username;


        public NetworkComponent(Socket socket, byte[]? civ = null, byte[]? siv = null)
        {
            UseGameCrypto = false;
            Socket = socket;
            RecvBuffer = new byte[1024];
            SendBuffer = new byte[850];
            ClientIV = civ ?? new byte[8];
            ServerIV = siv ?? new byte[8];
            Username = string.Empty;
            Random.Shared.NextBytes(ClientIV.Span);
            Random.Shared.NextBytes(ServerIV.Span);
        }
    }
}