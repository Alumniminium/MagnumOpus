using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MagnumOpus.Components;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Networking
{
    public static unsafe class Co2Packet
    {
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
        }

        public static T Deserialize<T>(Span<byte> buffer) where T : unmanaged
        {
            fixed (byte* ptr = buffer)
                return *(T*)ptr;
        }
    }
}