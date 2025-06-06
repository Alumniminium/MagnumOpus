using System.Runtime.InteropServices;
using MagnumOpus.Enums;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct MsgUserAttrib
    {
        public ushort Size;
        public ushort Id;
        public int UniqueId;
        public int Amount;
        public MsgUserAttribType Type;
        public ulong Value;

        public static MsgUserAttrib Create(int entityId, ulong value, MsgUserAttribType type)
        {
            var packet = new MsgUserAttrib
            {
                Size = (ushort)sizeof(MsgUserAttrib),
                Id = 1017,
                UniqueId = entityId,
                Amount = 1,
                Type = type,
                Value = value,
            };
            return packet;
        }
    }
}