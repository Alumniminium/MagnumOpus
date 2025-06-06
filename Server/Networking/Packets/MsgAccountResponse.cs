using System.Runtime.InteropServices;
using System.Text;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public unsafe struct MsgAccountResponse
    {
        [FieldOffset(0)]
        public ushort Size;
        [FieldOffset(2)]
        public ushort Id;
        [FieldOffset(4)]
        public int ClientId;
        [FieldOffset(8)]
        public int AuthCode;
        [FieldOffset(12)]
        public fixed byte ServerIP[16];
        [FieldOffset(28)]
        public ushort Port;

        public static MsgAccountResponse Create(string serverIP, ushort port, int key1, int key2)
        {
            var packet = new MsgAccountResponse
            {
                Size = (ushort)sizeof(MsgAccountResponse),
                Id = 1055,
                AuthCode = 2,
                ClientId = key2,
                Port = port
            };

            var ipBytes = Encoding.ASCII.GetBytes(serverIP);
            for (var i = 0; i < ipBytes.Length; i++)
                packet.ServerIP[i] = ipBytes[i];

            return packet;
        }
    }
}