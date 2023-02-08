using System.Net.Sockets;
using MagnumOpus.ECS;
using MagnumOpus.Networking.Cryptography;

namespace MagnumOpus.Components
{
    [Component]
    public struct NetworkComponent
    {
        public readonly int EntityId;
        public Socket Socket;
        public bool UseGameCrypto;
        public readonly Memory<byte> RecvBuffer;
        public readonly TQCipher AuthCrypto = new();
        public readonly BlowfishCipher GameCrypto = new();
        public readonly DiffieHellman DiffieHellman = new ();
        public readonly Memory<byte> ClientIV = new byte[8];
        public readonly Memory<byte> ServerIV = new byte[8];
        public string Username;

        public NetworkComponent(in NTT ntt, Socket socket)
        {
            UseGameCrypto=false;
            EntityId = ntt.Id;
            Socket = socket;
            RecvBuffer = new byte[1024];
            ClientIV = new byte[8];
            ServerIV = new byte[8];
            Username = string.Empty;
            Random.Shared.NextBytes(ClientIV.Span);
            Random.Shared.NextBytes(ServerIV.Span);
        }
        public override int GetHashCode()
        {
            return EntityId;
        }
    }
}