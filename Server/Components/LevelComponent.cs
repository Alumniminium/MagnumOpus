using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;

namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct LevelComponent(in NTT ntt, byte level = 1, uint experience = 0, uint experienceToNextLevel = 120)
    {
        public NTT NTT = ntt;
        public long ChangedTick = NttWorld.Tick;
        private byte _level = level;
        private uint _experience = experience;

        public uint ExperienceToNextLevel = experienceToNextLevel;

        public byte Level 
        { 
            readonly get => _level;
            set => NetworkSyncHelper.UpdateSyncedField(ref this, ref _level, value, MsgUserAttribType.Level, NTT);
        }

        public uint Experience 
        { 
            readonly get => _experience;
            set => NetworkSyncHelper.UpdateSyncedField(ref this, ref _experience, value, MsgUserAttribType.Experience, NTT);
        }

        public override readonly int GetHashCode() => NTT.Id;
    }
}