<<<<<<< HEAD
using System;
using System.Buffers;
using System.Diagnostics;
=======
>>>>>>> 3161883ebe68e1efedc7baa80d392375ebd53323
using System.Runtime.InteropServices;
using MagnumOpus.Components;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Networking
{
    public static unsafe class Co2Packet
    {
<<<<<<< HEAD
        public static void Serialize<T>(ref NetworkComponent net, ref T packetStruct) where T : unmanaged
        {
            lock(net.Username)
            {
                var size = typeof(T) == typeof(MsgDHX) ? 355 : sizeof(T);
                var requiredSize = net.SendBufferOffset + size;

                if (requiredSize > net.SendBuffer.Length)
                    Debugger.Break();

                var buffer = net.SendBuffer.Span.Slice(net.SendBufferOffset, size);
                MemoryMarshal.Write(buffer, ref packetStruct);

                // Increment SendBufferOffset
                var actualSize = typeof(T) == typeof(MsgDHX) ? 355 : BitConverter.ToUInt16(buffer[..2]);
                net.SendBufferOffset += actualSize;
            }
=======
        public static Memory<byte> Serialize<T>(ref T pacetStruct) where T : unmanaged
        {
            var buffer = new byte[typeof(T) == typeof(MsgDHX) ? 355 : sizeof(T)];
            MemoryMarshal.Write(buffer, ref pacetStruct);
            var size = typeof(T) == typeof(MsgDHX) ? 355 : BitConverter.ToUInt16(buffer, 0);

            return buffer.AsMemory()[0..size];
>>>>>>> 3161883ebe68e1efedc7baa80d392375ebd53323
        }

        public static T Deserialize<T>(Span<byte> buffer) where T : unmanaged
        {
            fixed (byte* ptr = buffer)
                return *(T*)ptr;
        }
    }
}