namespace MagnumOpus.Enums;

[Flags]
public enum MagicTypeEnum
{
    /// <summary>
    /// Damage a single target
    /// Thunder, Fire, Tornado, Meteor, ...
    /// </summary>
    DamageSingle = 1,
    /// <summary>
    /// Heal a single target
    /// Cure, Advanced Cure, Nectar, ...
    HealSingle = 2,
    /// <summary>
    /// Unknown
    /// </summary>
    AttackCrossHp = 3,
    /// <summary>
    /// Unknown
    /// </summary>
    AttackSectorHp = 4,
    /// <summary>
    /// Unknown
    /// </summary>
    AttackRoundHp = 5,
    AttackSingleStatus = 6,
    RecoverSingleStatus = 7,
    Square = 8,
    Jumpattack = 9,
    Randomtrans = 10,
    Dispatchxp = 11,
    Collide = 12,
    Serialcut = 13,
    Line = 14,
    Atkrange = 15,
    Atkstatus = 16,
    CallTeammember = 17,
    Recordtransspell = 18, // record map position to trans spell,
    Transform = 19,
    Addmana = 20, // support self target only,
    Laytrap = 21,
    Dance = 22,
    Callpet = 23,
    Vampire = 24, //power is percent award, use for call pet
    Instead = 25, //use for call pet
    Declife = 26,
    Groundsting = 27,
    Reborn = 28,
    TeamMagic = 29,
    BombLockall = 30,
    SorbSoul = 31,
    Steal = 32,
    LinePenetrable = 33,
    BlastThunder = 34,
    MultiAttachstatus = 35,
    MultiDetachstatus = 36,
    MultiCure = 37,
    StealMoney = 38,
    Ko = 39,
    Escape = 40,
    FlashAttack = 41,
    AttrackMonster = 42,
}