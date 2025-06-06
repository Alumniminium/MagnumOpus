using System.Runtime.InteropServices;
using MagnumOpus.Enums;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgSyndicate
    {
        public ushort Size;
        public ushort Id;
        public GuildRequest Type;
        public int Param;

        public static MsgSyndicate Create(int param, GuildRequest type)
        {
            var msg = new MsgSyndicate
            {
                Size = (ushort)sizeof(MsgSyndicate),
                Id = 1107,
                Type = type,
                Param = param
            };
            return msg;
        }
    }
}