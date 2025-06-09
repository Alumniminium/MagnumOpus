namespace MagnumOpus.Enums;

public enum MsgUserAttribType
{
    /// <summary>
    /// The player's current health
    /// </summary>
    Health = 0,
    /// <summary>
    /// The player's maximum health
    /// </summary>
    MaxHealth = 1,
    /// <summary>
    /// The player's current mana    
    /// </summary>
    Mana = 2,
    /// <summary>
    /// The player's maximum mana
    /// </summary>
    MaxMana = 3,
    /// <summary>
    /// The player's current amount of money in their inventory
    /// </summary>
    MoneyInventory = 4,
    /// <summary>
    /// The player's current amount of experience
    /// </summary>
    Experience = 5,
    /// <summary>
    /// The player's current amount of PK points
    /// </summary>
    PkPoints = 6,
    /// <summary>
    /// The player's class (Profession)
    /// </summary>
    Class = 7,
    /// <summary>
    /// Unknown
    /// </summary>
    Modifier = 8,
    /// <summary>
    /// The player's current stamina
    /// </summary>
    Stamina = 9,
    /// <summary>
    /// The player's current unused stat points
    /// </summary>
    StatPoints = 11,
    /// <summary>
    /// The player's current Mesh Id
    /// </summary>
    Look = 12,
    /// <summary>
    /// The player's current level
    /// </summary>
    Level = 13,
    /// <summary>
    /// The player's current spirit attribute points
    /// </summary>
    Spirit = 14,
    /// <summary>
    /// The player's current vitality attribute points
    /// </summary>
    Vitality = 15,
    /// <summary>
    /// The player's current strength attribute points
    /// </summary>
    Strength = 16,
    /// <summary>
    /// The player's current agility attribute points
    /// </summary>
    Agility = 17,
    /// <summary>
    /// The player's current LBless Timer (Revive Here)
    /// </summary>
    BlessTimer = 18,
    /// <summary>
    /// The player's current double exp timer
    /// </summary>
    ExpTimer = 19,
    /// <summary>
    /// Maybe the time for the name to flash blue? Maybe just a toggle?
    /// </summary>
    BlueTimer = 20,
    /// <summary>
    /// The player's current cursed time
    /// </summary>
    CurseTime = 21,
    /// <summary>
    /// Unknown, related to XP i think
    /// </summary>
    TimeAdd = 22,
    /// <summary>
    /// The player's current metempsychosis (Reborn Count)
    Metempsychosis = 23,
    /// <summary>
    /// The player's current status effects, Flags. Can be multiple
    /// Example: Flying, Superman, Cyclone, Transform, etc.
    /// </summary>
    StatusEffect = 26,
    /// <summary>
    /// The player's current hair style
    /// </summary>
    HairStyle = 27,
    /// <summary>
    /// The player's current amount of XP in the circle
    /// </summary>
    XpCircle = 28,
    /// <summary>
    /// The player's current lucky time timer
    /// </summary>
    LuckyTimeTimer = 29,
    /// <summary>
    /// The player's current amount of premium currency in their inventory 
    /// </summary>
    CPsInventory = 30,
    /// <summary>
    /// The player's current XP timer
    /// </summary>
    XpTimer = 31,
    /// <summary>
    /// The player's current training points for offline training
    /// </summary>
    TrainingPoints = 32,
    /// <summary>
    /// The player's current Nobility Rank
    /// </summary>
    Nobility = 81
}