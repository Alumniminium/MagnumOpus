using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Networking
{
    public static unsafe class Co2Packet
    {
        public static byte[] Serialize<T>(ref T packetStruct) where T : unmanaged
        {
            if (typeof(T) == typeof(MsgDHX))
                return SerializeDHX(ref packetStruct);

            var size = sizeof(T);
            var buffer = new byte[size];

            fixed (byte* pBuffer = buffer)
            {
                var pPacketStruct = (T*)pBuffer;
                *pPacketStruct = packetStruct;
            }

            size = BitConverter.ToUInt16(buffer, 0);
            return buffer[0..size];
        }

        public static byte[] SerializeDHX<T>(ref T packetStruct) where T : unmanaged
        {
            var size = sizeof(T);
            var buffer = new byte[size];

            fixed (byte* pBuffer = buffer)
            {
                var pPacketStruct = (T*)pBuffer;
                *pPacketStruct = packetStruct;
            }
            return buffer[..355];
        }


        public static T Deserialize<T>(Span<byte> buffer) where T : unmanaged
        {
            fixed (byte* ptr = buffer)
                return *(T*)ptr;
        }
    }
}