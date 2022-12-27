namespace MagnumOpus.Enums
{
    [Flags]
    public enum MapFlags : uint
    {
        None                = 0b0, // MAPTYPE_NORMAL
        NoPkpNoFlash        = 0b1, // MAPTYPE_PKFIELD: No PKPoints, Not Flashing...
        NoChangeMap         = 0b10, // MAPTYPE_CHGMAP_DISABLE: Can't change map
        RecordDisable       = 0b100, // MAPTYPE_RECORD_DISABLE: Do not save this position, save the previous
        NoPk                = 0b1000, // MAPTYPE_PK_DISABLE: Can't PK
        EnablePlayerShop    = 0b10000, // MAPTYPE_BOOTH_ENABLE: Can create booth
        DisableTeams        = 0b100000, // MAPTYPE_TEAM_DISABLE: Can't create team
        DisableScrolls      = 0b1000000, // MAPTYPE_TELEPORT_DISABLE: Can't use scroll
        GuildMap            = 0b10000000, // MAPTYPE_SYN_MAP: Syndicate MapId
        Prison              = 0b100000000, // MAPTYPE_PRISON_MAP: Prison MapId
        DisableFly          = 0b1000000000, // MAPTYPE_WING_DISABLE: Can't fly
        Family              = 0b10000000000, // MAPTYPE_FAMILY: Family MapId
        Mine                = 0b100000000000, // MAPTYPE_MINEFIELD: Mine MapId
        MAPTYPE_PKGAME      = 0b1000000000000, // MAPTYPE_PKGAME: PK Game MapId
        MAPTYPE_NEVERWOUND  = 0b10000000000000, // MAPTYPE_NEVERWOUND: Never Wound
        NewbieProtect       = 0b100000000000000, // MAPTYPE_DEADISLAND: Newbie protection
    }
}