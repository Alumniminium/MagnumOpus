using Co2Core.IO;
using MagnumOpus.Enums;
using NttECS.ECS;

namespace MagnumOpus.Components;

[Component]
/// <summary>
/// Magic spell targeting component defining spell targeting parameters and area-of-effect shapes.
/// Contains target coordinates, magic type data, and targeting geometry (Circle, Line, Sector).
/// Not saved to database (no SaveEnabled). Used by TargetFinderSystem to identify entities within
/// spell range and MagicAttackRoutingSystem for spell targeting logic. Essential for magic
/// combat targeting, spell range validation, and area-of-effect calculations.
/// </summary>
public struct TargetingComponent(ushort x, ushort y, MagicType.Entry magicType, TargetingType targetingType)
{
    public MagicType.Entry MagicType = magicType;
    public ushort X = x;
    public ushort Y = y;
    public TargetingType TargetingType = targetingType;
}