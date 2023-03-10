using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;
namespace MagnumOpus.Components.Leveling
{
    [Component]
    [Save]
    public struct LevelComponent
    {
        public readonly int EntityId;
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
                ref readonly var ntt = ref NttWorld.GetEntity(EntityId);
                var packet = MsgUserAttrib.Create(ntt.Id, level, MsgUserAttribType.Level);
                ntt.NetSync(ref packet, true);
            }
        }
        public uint Experience
        {
            get => experience; set
            {
                experience = value;
                ChangedTick = NttWorld.Tick;
                ref readonly var ntt = ref NttWorld.GetEntity(EntityId);
                var packet = MsgUserAttrib.Create(ntt.Id, experience, MsgUserAttribType.Experience);
                ntt.NetSync(ref packet, true);
            }
        }

        public LevelComponent(byte level = 1, uint experience = 0, uint experienceToNextLevel = 120)
        {
            Level = level;
            Experience = experience;
            ExperienceToNextLevel = experienceToNextLevel;
            ChangedTick = NttWorld.Tick;
        }
        public override int GetHashCode() => EntityId;
    }
}