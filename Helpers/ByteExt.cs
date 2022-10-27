using System.Globalization;

namespace MagnumOpus.Helpers
{
    public static class Constants
    {
        public static readonly sbyte[] DeltaX = {0, -1, -1, -1, 0, 1, 1, 1};
        public static readonly sbyte[] DeltaY = {1, 1, 0, -1, -1, -1, 0, 1};
    }
    public static class ByteExt
    {
        public static string Dump(this in Memory<byte> packet)
        {
            string Hex = "";
            foreach (byte b in packet.Span)
                Hex = Hex + b.ToString("X2") + " ";
            
            string Out = "";
            while (Hex.Length != 0)
            {
                int SubLength = Hex.Length >= 48 ? 48 : Hex.Length;
                string SubString = Hex[..SubLength];
                int Remove = SubString.Length;
                SubString = SubString.PadRight(50, ' ') + StrHexToAnsi(SubString);
                Hex = Hex.Remove(0, Remove);
                Out = Out + SubString + "\r\n";
            }
            return Out;
        }

        private static string StrHexToAnsi(string StrHex)
        {
            string[] Data = StrHex.Split(new char[] { ' ' });
            string Ansi = "";
            foreach (string tmpHex in Data)
            {
                if (tmpHex == "")
                    continue;
                
                byte ByteData = byte.Parse(tmpHex, NumberStyles.HexNumber);
                if ((ByteData >= 32) & (ByteData <= 126))
                    Ansi += ((char)ByteData).ToString();
                else
                    Ansi += ".";
            }
            return Ansi;
        }
    }
}