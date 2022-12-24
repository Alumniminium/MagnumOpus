using System.Buffers;
using System.Runtime.InteropServices;
using MagnumOpus.Components;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgProf
    {
        public ushort Size;
        public ushort Id;
        public uint ProfId;
        public uint Level;
        public uint Experience;

        public static MsgProf Create(ProfComponent prof)
        {
            var packet = new MsgProf
            {
                Size = (ushort)sizeof(MsgProf),
                Id = 1025,
                ProfId = prof.Id,
                Experience = prof.Experience,
                Level = prof.Level
            };
            return packet;
        }
    }
}