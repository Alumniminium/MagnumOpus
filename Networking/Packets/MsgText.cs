using System.Runtime.InteropServices;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1, CharSet = CharSet.Ansi)]
    public struct MsgText
    {
        public static unsafe Memory<byte> Create(string from, string to, string message, MsgTextType type)
        {
            var writer = new PacketWriter();
            writer.Write((ushort)1004);
            writer.Write(0);
            writer.Write((ushort)type);
            writer.Write((ushort)0);
            writer.Write(0);
            writer.Write(0);
            writer.Write(0);
            writer.Write(new List<string>
            {
                from,
                to,
                string.Empty,
                message
            });
            return writer.ToArray();
        }
    }
}