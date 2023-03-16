using System;
using System.Buffers;
using System.Runtime.InteropServices;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Networking
{
    public static unsafe class Co2Packet
    {
        public static byte[] Serialize<T>(ref T pacetStruct) where T : unmanaged
        {
            var size = typeof(T) == typeof(MsgDHX) ? 355 : sizeof(T);
            var buffer = ArrayPool<byte>.Shared.Rent(size);
            MemoryMarshal.Write(buffer, ref pacetStruct);
            var actualSize = typeof(T) == typeof(MsgDHX) ? 355 : BitConverter.ToUInt16(buffer, 0);

            return buffer[0..actualSize];
        }

        public static T Deserialize<T>(Span<byte> buffer) where T : unmanaged
        {
            fixed (byte* ptr = buffer)
                return *(T*)ptr;
        }
    }
}