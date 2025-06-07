using NttECS.ECS;

namespace MagnumOpus.Components;

[Component]
/// <summary>
/// Transient magic spell casting request component containing spell parameters and targeting
/// information. Stores skill ID, target entity, position coordinates, and cooldown ticks.
/// Not saved to database (no SaveEnabled). Processed by MagicAttackSystem to execute spells,
/// validate targets, apply effects, and handle spell routing. Part of the magic combat pipeline.
/// </summary>
public struct MagicAttackRequestComponent(int skillId, int targetId, ushort x, ushort y, int sleepTicks)
{
    public int SkillId = skillId;
    public int TargetId = targetId;
    public ushort X = x;
    public ushort Y = y;
    public int SleepTicks = sleepTicks;
}