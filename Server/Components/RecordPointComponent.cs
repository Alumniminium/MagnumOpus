using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
/// <summary>
/// Player record point component storing saved location for return teleportation. Contains
/// map ID and coordinates of a player's recorded position for recall abilities or emergency
/// teleportation. Currently defined but not actively processed by any systems - represents
/// planned recall/teleport system for players to mark and return to specific locations,
/// similar to town portal or homepoint mechanics.
/// </summary>
public struct RecordPointComponent(ushort x, ushort y, ushort map)
{
    public ushort Map = map;
    public ushort X = x;
    public ushort Y = y;
}