namespace MagnumOpus.Enums;

public enum ClasseName : byte
{
    /// <summary>
    /// The 'Dual Wielding' class in the game.
    /// </summary>
    InternTrojan = 10,
    Trojan = 11,
    VeteranTrojan = 12,
    TigerTrojan = 13,
    DragonTrojan = 14,
    TrojanMaster = 15,

    /// <summary>
    /// The 'Shield Wielding' class in the game.
    /// </summary>
    InternWarrior = 20,
    Warrior = 21,
    BrassWarrior = 22,
    SilverWarrior = 23,
    GoldWarrior = 24,
    WarriorMaster = 25,

    /// <summary>
    /// The 'Bow Wielding' class in the game.
    /// </summary>
    InternArcher = 40,
    Archer = 41,
    EagleArcher = 42,
    TigerArcher = 43,
    DragonArcher = 44,
    ArcherMaster = 45,

    /// <summary>
    /// The 'Mage' class in the game.
    /// On Level 40, the player will be able to pick the water (support) or fire (dps) progression 
    /// </summary>
    InternTaoist = 100,
    Taoist = 101,

    /// <summary>
    /// The 'Support Mage' in the game.
    /// </summary>
    WaterTaoist = 132,
    WaterWizard = 133,
    WaterMaster = 134,
    WaterSaint = 135,

    /// <summary>
    /// The 'DPS Mage' in the game.
    /// </summary>
    FireTaoist = 142,
    FireWizard = 143,
    FireMaster = 144,
    FireSaint = 145
}