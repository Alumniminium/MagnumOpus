using System.Globalization;

namespace MagnumOpus.Helpers
{
    /// <summary>
    /// Extension methods for byte arrays and spans providing hex dump functionality for network packet debugging.
    /// Formats binary data into readable hex output with ASCII representation for debugging network protocols.
    /// </summary>
    public static class ByteExt
    {
        /// <summary>
        /// Converts a byte span into a formatted hex dump with ASCII representation for debugging.
        /// </summary>
        /// <param name="packet">Byte span to convert to hex dump format</param>
        /// <returns>Formatted hex dump string with ASCII representation</returns>
        public static string Dump(this Span<byte> packet)
        {
            var hexString = "";
            foreach (var byteValue in packet)
                hexString = hexString + byteValue.ToString("X2") + " ";

            var output = "";
            while (hexString.Length != 0)
            {
                var lineLength = hexString.Length >= 48 ? 48 : hexString.Length;
                var hexLine = hexString[..lineLength];
                var removeLength = hexLine.Length;
                hexLine = hexLine.PadRight(50, ' ') + StrHexToAnsi(hexLine);
                hexString = hexString.Remove(0, removeLength);
                output = output + hexLine + "\r\n";
            }
            return output;
        }
        /// <summary>
        /// Converts a memory buffer into a formatted hex dump with ASCII representation for debugging.
        /// </summary>
        /// <param name="packet">Memory buffer to convert to hex dump format</param>
        /// <returns>Formatted hex dump string with ASCII representation</returns>
        public static string Dump(this in Memory<byte> packet) => Dump(packet.Span);

        /// <summary>
        /// Converts hex string to ASCII representation, replacing non-printable characters with dots.
        /// </summary>
        /// <param name="hexString">Hex string to convert to ASCII</param>
        /// <returns>ASCII representation with dots for non-printable characters</returns>
        private static string StrHexToAnsi(string hexString)
        {
            var hexBytes = hexString.Split([' ']);
            var asciiOutput = "";
            foreach (var hexByte in hexBytes)
            {
                if (hexByte == "")
                    continue;

                var byteValue = byte.Parse(hexByte, NumberStyles.HexNumber);
                if ((byteValue >= 32) & (byteValue <= 126))
                    asciiOutput += ((char)byteValue).ToString();
                else
                    asciiOutput += ".";
            }
            return asciiOutput;
        }
    }
}