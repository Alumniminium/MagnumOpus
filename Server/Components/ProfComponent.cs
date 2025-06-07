using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
/// <summary>
/// Profession/skill component tracking individual skill progression and experience. Contains
/// skill ID, current level, accumulated experience, and experience required for next level
/// with change tracking for network updates. Currently defined but not actively processed
/// by any systems - represents planned skill system for profession-based character development,
/// crafting abilities, and specialized skill progression beyond basic character levels.
/// </summary>
public struct ProfComponent(ushort skillId, ushort level, ushort experience, ushort experienceToNextLevel)
{
    public long ChangedTick = NttWorld.Tick;
    public ushort Id = skillId;
    public ushort Level = level;
    public ushort Experience = experience;
    public ushort ExperienceToNextLevel = experienceToNextLevel;
}