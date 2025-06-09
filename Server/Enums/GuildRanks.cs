namespace MagnumOpus.Enums;

public enum GuildRanks : byte
{
    /// <summary>
    /// Not a member of a guild
    /// </summary>
    None = 0,
    /// <summary>
    /// Member of a guild
    /// </summary>
    Member = 50,
    /// <summary>
    /// Not sure
    /// </summary>
    InternManager = 60,
    /// <summary>
    /// Not sure
    /// </summary>
    DeputyManager = 70,
    /// <summary>
    /// Not sure
    /// </summary>
    BranchManager = 80,
    /// <summary>
    /// Not sure
    /// </summary>
    DeputyLeader = 90,
    /// <summary>
    /// Guild leader
    /// </summary>
    Leader = 100
}