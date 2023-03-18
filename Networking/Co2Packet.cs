using System.Runtime.InteropServices;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Networking
{
    public static unsafe class Co2Packet
    {
        public static Memory<byte> Serialize<T>(ref T pacetStruct) where T : unmanaged
        {
            var buffer = new byte[typeof(T) == typeof(MsgDHX) ? 355 : sizeof(T)];
            MemoryMarshal.Write(buffer, ref pacetStruct);
            var size = typeof(T) == typeof(MsgDHX) ? 355 : BitConverter.ToUInt16(buffer, 0);

            return buffer.AsMemory()[0..size];
        }

        public static T Deserialize<T>(Span<byte> buffer) where T : unmanaged
        {
            fixed (byte* ptr = buffer)
                return *(T*)ptr;
        }
    }
}