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

        public static byte[] Create(uint mapId, uint flags)
        {
            var packet = stackalloc MsgStatus[1];
            packet->Size = (ushort)sizeof(MsgStatus);
            packet->Id = 1110;
            packet->MapId = mapId;
            packet->DynMapId = mapId;
            packet->Flags = flags;

            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(MsgUpdate));
            fixed (byte* p = buffer)
                *(MsgStatus*)p = *packet;
            return buffer;
        }

        public static implicit operator byte[](MsgStatus msg)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(MsgUpdate));
            fixed (byte* p = buffer)
                *(MsgStatus*)p = *&msg;
            return buffer;
        }
    }
}