namespace MagnumOpus.Enums;

public enum PkMode : byte
{
    /// <summary>
    /// Attack anything
    /// </summary>
    Kill = 0,
    /// <summary>
    /// Attack only Monsters
    /// Excl. Guards
    /// </summary>
    Peace = 1,

    /// <summary>
    /// Attack anything except Teammates
    /// Incl. Guards
    /// </summary>
    Team = 2,
    /// <summary>
    /// Attack anything legal to attack
    /// Blue/Black name (incl. Team Members), Monsters
    /// Not Guards.
    Capture = 3
}