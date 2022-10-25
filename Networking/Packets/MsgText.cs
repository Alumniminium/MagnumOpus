using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Simulation.Components;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1, CharSet = CharSet.Ansi)]
    public struct MsgText
    {
        public static unsafe byte[] Create(string from, string to, string message, MsgTextType type)
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
            var msg = writer.ToArray();
            Array.Resize(ref msg, msg.Length + 8);
            var tqserver = "TQServer".ToCharArray();
            for (var i = 0; i < 8; i++)
                msg[msg.Length - 8 + i] = (byte)tqserver[i];
            return msg;
        }
    }
}