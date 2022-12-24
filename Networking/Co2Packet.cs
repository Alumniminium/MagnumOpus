using System.Runtime.InteropServices;

namespace MagnumOpus.Networking
{
    public static unsafe class Co2Packet
    {
        public static Memory<byte> Serialze<T>(ref T pacetStruct) where T : unmanaged
        {
            var buffer = new byte[sizeof(T)];
            MemoryMarshal.Write(buffer, ref pacetStruct);
            return buffer;
        }
        public static Memory<byte> Serialze<T>(ref T pacetStruct, int size) where T : unmanaged
        {
            var buffer = new byte[size];
            fixed (byte* ptr = buffer)
                *(T*)ptr = pacetStruct;
            return buffer;
        }

        public static T Deserialze<T>(in Memory<byte> buffer) where T : unmanaged 
        {
            fixed (byte* ptr = buffer.Span)
                return *(T*)ptr;
        }
    }
}