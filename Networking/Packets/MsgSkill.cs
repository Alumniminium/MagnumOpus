using System.Buffers;
using System.Runtime.InteropServices;
using MagnumOpus.Components;
using SQLitePCL;

namespace MagnumOpus.Networking.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MsgSkill
    {
        public ushort Size;
        public ushort Id;
        public uint Experience;
        public ushort SkillId;
        public ushort Level;

        public static MsgSkill Create(ushort skillId, uint exp, ushort lvl)
        {
            var packet = new MsgSkill
            {
                Size = (ushort)sizeof(MsgSkill),
                Id = 1103,
                SkillId = skillId,
                Experience = exp,
                Level = lvl
            };
            return packet;
        }
    }
}