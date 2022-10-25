using System.Buffers;
using System.Runtime.InteropServices;
using MagnumOpus.Enums;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgTrade
    {
        public ushort Size;
        public ushort Id;
        public int UniqueId;
        public TradeMode Mode;

        public static MsgTrade Create(int uniqueId, TradeMode mode)
        {
            var packet = new MsgTrade
            {
                Size = (ushort)sizeof(MsgTrade),
                Id = 1056,
                UniqueId = uniqueId,
                Mode = mode,
            };
            return packet;
        }

        public static unsafe implicit operator byte[](MsgTrade msg)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(sizeof(MsgUpdate));
            fixed (byte* p = buffer)
                *(MsgTrade*)p = *&msg;
            return buffer;
        }
    }
}