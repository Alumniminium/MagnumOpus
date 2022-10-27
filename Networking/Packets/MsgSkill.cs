using System.Buffers;
using System.Runtime.InteropServices;
using MagnumOpus.Components;

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

        public static Memory<byte> Create(SkillComponent skill)
        {
            var packet = new MsgSkill
            {
                Size = (ushort)sizeof(MsgSkill),
                Id = 1103,
                SkillId = skill.Id,
                Experience = skill.Experience,
                Level = skill.Level
            };
            return packet;
        }

        public static unsafe implicit operator Memory<byte>(MsgSkill msg)
        {
            var buffer = new byte[sizeof(MsgSkill)];
            fixed (byte* p = buffer)
                *(MsgSkill*)p = *&msg;
            return buffer;
        }
    }
}