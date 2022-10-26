using System.Buffers;
using MagnumOpus.Enums;

namespace MagnumOpus.Networking.Packets
{
    public unsafe struct MsgWalk
    {
        public ushort Size;
        public ushort Id;
        public int UniqueId;
        public Direction Direction;
        public bool Running;
        public ushort Unknown;

        public static Memory<byte> Create(int uniqueId, Direction direction, bool running)
        {
            MsgWalk* msgP = stackalloc MsgWalk[1];

            msgP->Size = (ushort)sizeof(MsgWalk);
            msgP->Id = 1005;
            msgP->UniqueId = uniqueId;
            msgP->Direction = direction;
            msgP->Running = running;
            msgP->Unknown = 0;

            var buffer = new byte[sizeof(MsgWalk)];
            fixed (byte* p = buffer)
                *(MsgWalk*)p = *msgP;
            return buffer;
        }

        public static implicit operator Memory<byte>(MsgWalk msg)
        {
            var buffer = new byte[sizeof(MsgWalk)];
            fixed (byte* p = buffer)
                *(MsgWalk*)p = *&msg;
            return buffer;
        }
    }
}