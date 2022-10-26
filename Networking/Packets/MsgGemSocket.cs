using System.Buffers;
using System.Runtime.InteropServices;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgGemSocket
    {
        public ushort Size;
        public ushort Id;
        public int UnqiueId;
        public int ItemUID;
        public int GemUID;
        public ushort SocketID;
        public ushort RemoveGem;

        public static Memory<byte> Create(int uid, int itemuid, int gemuid, ushort socketid, ushort removegem)
        {
            var msgP = stackalloc MsgGemSocket[1];
            {
                msgP->Size = (ushort)sizeof(MsgGemSocket);
                msgP->Id = 1027;
                msgP->UnqiueId = uid;
                msgP->ItemUID = itemuid;
                msgP->GemUID = gemuid;
                msgP->SocketID = socketid;
                msgP->RemoveGem = removegem;
            }

            var buffer = new byte[sizeof(MsgGemSocket)];
            fixed (byte* p = buffer)
                *(MsgGemSocket*)p = *msgP;
            return buffer;
        }
    }
}