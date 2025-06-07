using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
/// <summary>
/// Character progression component managing player level and experience points. Contains current
/// level, accumulated experience, and experience required for next level with network synchronization.
/// Central to character development - used by LevelingSystem for progression logic, attribute
/// allocation, and level-up benefits. Also used by TeamSystem to determine experience sharing
/// eligibility within level ranges for balanced party play.
/// </summary>
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
        set => NetworkHelper.UpdateSyncedField(ref this, ref _level, value, MsgUserAttribType.Level, NTT);
    }

    public uint Experience
    {
        readonly get => _experience;
        set => NetworkHelper.UpdateSyncedField(ref this, ref _experience, value, MsgUserAttribType.Experience, NTT);
    }

    public override readonly int GetHashCode() => NTT.Id;
}