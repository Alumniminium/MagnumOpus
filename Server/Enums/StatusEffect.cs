namespace MagnumOpus.Enums;

[Flags]
public enum StatusEffect : long
{
    None = 0x0,
    /// <summary>
    /// Blue flashing name
    /// </summary>
    Flashing = 0x1,
    Poisoned = 0x2,
    Unknow = 0x4,
    Unknow2 = 0x8,
    /// <summary>
    /// Shows the XP Skills
    /// </summary>
    XpList = 0x10,
    FrozenRemoveName = 0x20,
    /// <summary>
    /// Show Golden Star above Head
    /// </summary>
    TeamLeader = 0x40,
    StarOfAccuracy = 0x80,
    MagicShield = 0x100,
    Stigma = 0x200,
    Dead = 0x400,
    /// <summary>
    /// Starts fading out the entity until invisible
    /// </summary>
    Fade = 0x800,
    XpAccuracy = 0x1000,
    XpShield = 0x2000,
    RedName = 0x4000,
    BlackName = 0x8000,
    SpawnProtection = 0x10000,
    SuperMan = 0x40000,
    Invisibility = 0x400000,
    Cyclone = 0x800000,
    Flying = 0x8000000,
    CastingPray = 0x40000000,
    Praying = 0x80000000,
}