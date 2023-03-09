using System.Collections.Concurrent;
using System.Net.Sockets;
using MagnumOpus.ECS;
using MagnumOpus.Networking.Cryptography;
using Newtonsoft.Json;

namespace MagnumOpus.Components
{
    [Component]
    public struct NetworkComponent
    {
        public Socket Socket;
        public bool UseGameCrypto;
        public readonly Memory<byte> RecvBuffer;
        public readonly TQCipher AuthCrypto = new();
        public BlowfishCipher GameCrypto = new();
        public readonly DiffieHellman DiffieHellman = new();
        public readonly Memory<byte> ClientIV = new byte[8];
        public readonly Memory<byte> ServerIV = new byte[8];
        public readonly ConcurrentQueue<Memory<byte>> SendQueue = new();
        public readonly ConcurrentQueue<Memory<byte>> RecvQueue = new();
        public string Username;


        [JsonConstructor]
        public NetworkComponent(Socket socket, byte[]? civ = null, byte[]? siv = null)
        {
            UseGameCrypto = false;
            Socket = socket;
            RecvBuffer = new byte[1024];
            ClientIV = civ ?? new byte[8];
            ServerIV = siv ?? new byte[8];
            Username = string.Empty;
            Random.Shared.NextBytes(ClientIV.Span);
            Random.Shared.NextBytes(ServerIV.Span);
        }
    }
}