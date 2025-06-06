using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct LevelComponent
    {
        public NTT NTT;
        public long ChangedTick = NttWorld.Tick;
        
        public byte Level;
        public uint Experience;
        public uint ExperienceToNextLevel;

        public LevelComponent(in NTT ntt, byte level = 1, uint experience = 0, uint experienceToNextLevel = 120)
        {
            NTT = ntt;
            Level = level;
            Experience = experience;
            ExperienceToNextLevel = experienceToNextLevel;
        }

        public void SetLevel(byte value)
        {
            if (Level != value)
            {
                Level = value;
                ChangedTick = NttWorld.Tick;
                var packet = MsgUserAttrib.Create(NTT.Id, value, MsgUserAttribType.Level);
                NTT.NetSync(ref packet, true);
            }
        }

        public void SetExperience(uint value)
        {
            if (Experience != value)
            {
                Experience = value;
                ChangedTick = NttWorld.Tick;
                var packet = MsgUserAttrib.Create(NTT.Id, value, MsgUserAttribType.Experience);
                NTT.NetSync(ref packet, true);
            }
        }

        public override int GetHashCode() => NTT;
    }
}