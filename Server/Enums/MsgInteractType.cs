namespace MagnumOpus.Enums;

public enum MsgInteractType
{
    /// <summary>
    /// Placeholder for no action.
    /// </summary>
    None = 0,

    /// <summary>
    /// Physical is always a single-target action.
    /// Physical never hits Dead targets.
    /// This causes melee attack animation on the client
    /// </summary>
    Physical = 2,

    /// <summary>
    /// Magic is *NOT* always a single-target action.
    /// Magic *MAY* hit Dead targets. (Revive)
    /// This causes the magic charge effect to play on the client
    /// </summary>
    Magic = 21,

    /// <summary>
    /// Ranged is always a single-target action.
    /// Ranged never hits Dead targets.
    /// This causes the bow animation to play on the client, even if no bow/arrow is equipped
    /// </summary>
    Ranged = 25,

    /// <summary>
    /// RequestMarriage is always a single-target action.
    /// RequestMarriage only works on players
    /// </summary>
    RequestMarriage = 8,

    /// <summary>
    /// AcceptMarriage is always a single-target action.
    /// AcceptMarriage only works on players
    /// </summary>
    AcceptMarriage = 9,

    /// <summary>
    /// Death is always a single-target action.
    /// Death works on any attackable entity
    /// Causes the death animation to play on the client and the 'flying experience orb' appear 
    /// </summary>
    Death = 14,

    /// <summary>
    /// This is still mysterious, it appears to update the MonsterHunter Jar thingy
    /// </summary>
    MonsterHunter = 30
}