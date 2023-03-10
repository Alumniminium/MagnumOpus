using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;
namespace MagnumOpus.Components.Leveling
{
    [Component]
    [Save]
    public struct LevelComponent
    {
        public  NTT NTT;
        public long ChangedTick;
        private byte level;
        private uint experience;
        public uint ExperienceToNextLevel;

        public byte Level
        {
            get => level; set
            {
                level = value;
                ChangedTick = NttWorld.Tick;
                var packet = MsgUserAttrib.Create(NTT.Id, level, MsgUserAttribType.Level);
                NTT.NetSync(ref packet, true);
            }
        }
        public uint Experience
        {
            get => experience; set
            {
                experience = value;
                ChangedTick = NttWorld.Tick;
                var packet = MsgUserAttrib.Create(NTT.Id, experience, MsgUserAttribType.Experience);
                NTT.NetSync(ref packet, true);
            }
        }

        public LevelComponent(in NTT ntt, byte level = 1, uint experience = 0, uint experienceToNextLevel = 120)
        {
            NTT = ntt;
            Level = level;
            Experience = experience;
            ExperienceToNextLevel = experienceToNextLevel;
            ChangedTick = NttWorld.Tick;
        }
        public override int GetHashCode() => NTT;
    }
}