using System.Globalization;

namespace MagnumOpus.Helpers
{
    public static class ByteExt
    {
        public static string Dump(this in Memory<byte> packet)
        {
            var Hex = "";
            foreach (var b in packet.Span)
                Hex = Hex + b.ToString("X2") + " ";

            var Out = "";
            while (Hex.Length != 0)
            {
                var SubLength = Hex.Length >= 48 ? 48 : Hex.Length;
                var SubString = Hex[..SubLength];
                var Remove = SubString.Length;
                SubString = SubString.PadRight(50, ' ') + StrHexToAnsi(SubString);
                Hex = Hex.Remove(0, Remove);
                Out = Out + SubString + "\r\n";
            }
            return Out;
        }

        private static string StrHexToAnsi(string StrHex)
        {
            var Data = StrHex.Split(new char[] { ' ' });
            var Ansi = "";
            foreach (var tmpHex in Data)
            {
                if (tmpHex == "")
                    continue;

                var ByteData = byte.Parse(tmpHex, NumberStyles.HexNumber);
                if ((ByteData >= 32) & (ByteData <= 126))
                    Ansi += ((char)ByteData).ToString();
                else
                    Ansi += ".";
            }
            return Ansi;
        }
    }
}