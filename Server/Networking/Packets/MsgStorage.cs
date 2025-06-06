using System.Runtime.InteropServices;
using MagnumOpus.Enums;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct MsgStorage
    {
        public ushort Length;
        public ushort Id;
        public int UniqueId;
        public MsgStorageAction Action;
        public MsgStorageType Type;
        public ushort Unknown2;
        public int Param;

        public static MsgStorage Create(int uniqueId, MsgStorageAction action)
        {
            var msg = new MsgStorage
            {
                Length = (ushort)sizeof(MsgStorage),
                Id = 1102,
                Param = 0,
                Action = action,
                UniqueId = uniqueId,
                Type = MsgStorageType.Storage,
                Unknown2 = 0,
            };
            return msg;
        }
    }
}