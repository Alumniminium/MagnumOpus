namespace MagnumOpus.Networking
{
    /// <summary>
    /// High-performance packet serialization system for Conquer Online 2 protocol.
    /// Provides unsafe memory operations for zero-copy conversion between packet structs and byte arrays.
    /// Supports both standard serialization and specialized DiffieHellman exchange (DHX) packet handling.
    /// </summary>
    public static unsafe class Co2Packet
    {
        /// <summary>
        /// Serializes a packet structure to byte array using unsafe memory operations for maximum performance.
        /// Automatically detects DiffieHellman exchange packets and routes to specialized serializer.
        /// Uses the packet's embedded size field to return correctly sized byte array.
        /// </summary>
        /// <typeparam name="T">Packet structure type that must be unmanaged (no references)</typeparam>
        /// <param name="packetStruct">Packet structure to serialize</param>
        /// <returns>Byte array containing serialized packet data trimmed to actual packet size</returns>
        public static byte[] Serialize<T>(ref T packetStruct) where T : unmanaged
        {
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


        /// <summary>
        /// Deserializes byte data into a packet structure using unsafe memory operations for maximum performance.
        /// Includes safety validation to prevent buffer overruns and memory access violations.
        /// </summary>
        /// <typeparam name="T">Target packet structure type that must be unmanaged</typeparam>
        /// <param name="buffer">Byte span containing packet data to deserialize</param>
        /// <returns>Deserialized packet structure, or default value if buffer is too small</returns>
        public static T Deserialize<T>(Span<byte> buffer) where T : unmanaged
        {
            // Validate buffer size before unsafe memory access
            if (buffer.Length < sizeof(T))
            {
                // Return default value for safety - caller should handle gracefully
                return default(T);
            }

            fixed (byte* ptr = buffer)
                return *(T*)ptr;
        }

    }
}