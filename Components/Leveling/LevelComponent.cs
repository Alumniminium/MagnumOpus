using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Components
{
    [Component]
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
                ref readonly var entity = ref NttWorld.GetEntity(EntityId);
                var packet = MsgUserAttrib.Create(entity.NetId, level, MsgUserAttribType.Level);
                entity.NetSync(ref packet, true);
            }
        }
        public uint Experience
        {
            get => experience; set
            {
                experience = value;
                ChangedTick = NttWorld.Tick;
                ref readonly var entity = ref NttWorld.GetEntity(EntityId);
                var packet = MsgUserAttrib.Create(entity.NetId, experience, MsgUserAttribType.Experience);
                entity.NetSync(ref packet, true);
            }
        }

        public LevelComponent(int entityId, byte level=1, uint experience=0, uint experienceToNextLevel=120)
        {
            EntityId = entityId;
            Level = level;
            Experience = experience;
            ExperienceToNextLevel = experienceToNextLevel;
            ChangedTick = NttWorld.Tick;
        }
        public override int GetHashCode() => EntityId;
    }
}