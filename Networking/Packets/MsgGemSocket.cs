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

        public static MsgGemSocket Create(int uid, int itemuid, int gemuid, ushort socketid, ushort removegem)
        {
            var msgP = new MsgGemSocket
            {
                Size = (ushort)sizeof(MsgGemSocket),
                Id = 1027,
                UnqiueId = uid,
                ItemUID = itemuid,
                GemUID = gemuid,
                SocketID = socketid,
                RemoveGem = removegem,
            };
            return msgP;
        }
    }
}