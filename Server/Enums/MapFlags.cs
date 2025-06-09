namespace MagnumOpus.Enums;

[Flags]
public enum MapFlags : uint
{
    None = 0b0,

    /// <summary>
    /// No blue flashing name when PKing on this Map,
    /// Gain no PK points on this map 
    /// Examples:  Arena, GuildWar
    /// </summary>
    NoPkpNoFlash = 0b1, // MAPTYPE_PKFIELD: No PKPoints, Not Flashing...

    /// <summary>
    /// Unknown MAPTYPE_CHGMAP_DISABLE: Can't change map
    /// </summary>
    NoChangeMap = 0b10,

    /// <summary>
    /// If you logout or diem you will not respawn on this map
    /// Examples:  Meteor Zone, Guild War
    /// </summary>
    RecordDisable = 0b100, // MAPTYPE_RECORD_DISABLE: Do not save this position, save the previous

    /// <summary>
    /// Can't PK on this map
    /// Examples:  Twin City
    /// </summary>
    NoPk = 0b1000, // MAPTYPE_PK_DISABLE: Can't PK

    /// <summary>
    /// Can create booth on this map
    /// Examples:  Market
    /// </summary>
    EnablePlayerShop = 0b10000, // MAPTYPE_BOOTH_ENABLE: Can create booth

    /// <summary>
    /// Can't create team on this map
    /// Examples: Unknown
    /// </summary>
    DisableTeams = 0b100000, // MAPTYPE_TEAM_DISABLE: Can't create team

    /// <summary>
    /// Can't use scrolls on this map
    /// Examples: Bot Jail
    /// </summary>
    DisableScrolls = 0b1000000, // MAPTYPE_TELEPORT_DISABLE: Can't use scroll

    /// <summary>
    /// Possibly Guildwar Map, not sure about effects, maybe scoreboard or something with Gates? 
    /// </summary>
    GuildMap = 0b10000000, // MAPTYPE_SYN_MAP: Syndicate MapId

    /// <summary>
    /// Possibly Prison Map, not sure about effects, maybe scoreboard or something with Gates?
    /// </summary>
    Prison = 0b100000000, // MAPTYPE_PRISON_MAP: Prison MapId

    /// <summary>
    /// Can't fly on this map
    /// Examples: Unknown
    /// </summary>
    DisableFly = 0b1000000000, // MAPTYPE_WING_DISABLE: Can't fly

    /// <summary>
    /// Not sure about effects.
    /// Examples: House Maps
    /// </summary>
    Family = 0b10000000000, // MAPTYPE_FAMILY: Family MapId

    /// <summary>
    /// Enables Right-Click to Mine with a Pickaxe
    /// Examples: Mine Maps
    /// </summary>
    Mine = 0b100000000000, // MAPTYPE_MINEFIELD: Mine MapId

    /// <summary>
    /// Unknown
    /// </summary>
    PkGame = 0b1000000000000, // MAPTYPE_PKGAME: PK Game MapId

    /// <summary>
    /// Unknown
    /// </summary>
    NeverWound = 0b10000000000000, // MAPTYPE_NEVERWOUND: Never Wound

    /// <summary>
    /// Unknown, enabled in Twin City iirc, I think the point is to prevent PK on low level players
    /// </summary>
    NewbieProtect = 0b100000000000000, // MAPTYPE_DEADISLAND: Newbie protection
}