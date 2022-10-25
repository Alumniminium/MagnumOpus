using System.Net.Sockets;
using MagnumOpus.ECS;
using MagnumOpus.Networking.Cryptography;

namespace MagnumOpus.Simulation.Components
{
    [Component]
    public struct NetworkComponent
    {
        public readonly int EntityId;
        public Socket Socket;
        public readonly Memory<byte> RecvBuffer;
        public readonly TQCipher AuthCrypto = new();
        public readonly BlowfishCipher GameCrypto = new();
        public readonly DiffieHellman DiffieHellman = new ();
        public readonly byte[] ClientIV = new byte[8];
        public readonly byte[] ServerIV = new byte[8];

        public NetworkComponent(int entityId, Socket socket)
        {
            EntityId = entityId;
            Socket = socket;
            RecvBuffer = new byte[1024];
            ClientIV = new byte[8];
            ServerIV = new byte[8];

            Random.Shared.NextBytes(ClientIV);
            Random.Shared.NextBytes(ServerIV);
        }
        public override int GetHashCode()
        {
            return EntityId;
        }
    }
}