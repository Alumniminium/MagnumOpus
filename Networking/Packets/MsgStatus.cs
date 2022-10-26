using System.Buffers;
using System.Runtime.InteropServices;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgStatus
    {
        public ushort Size;
        public ushort Id;
        public uint MapId;
        public uint DynMapId;
        public uint Flags;

        public static Memory<byte> Create(uint mapId, uint flags)
        {
            var packet = stackalloc MsgStatus[1];
            packet->Size = (ushort)sizeof(MsgStatus);
            packet->Id = 1110;
            packet->MapId = mapId;
            packet->DynMapId = mapId;
            packet->Flags = flags;

            var buffer = new byte[sizeof(MsgStatus)];
            fixed (byte* p = buffer)
                *(MsgStatus*)p = *packet;
            return buffer;
        }

        public static implicit operator Memory<byte>(MsgStatus msg)
        {
            var buffer = new byte[sizeof(MsgStatus)];
            fixed (byte* p = buffer)
                *(MsgStatus*)p = *&msg;
            return buffer;
        }
    }
}