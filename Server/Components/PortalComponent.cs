using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
/// <summary>
/// Portal teleportation component that defines portal destination coordinates. Contains target
/// X and Y position for portal transportation. Used by PortalSystem to teleport entities that
/// interact with portal entities. When activated, entities are moved to the specified coordinates,
/// enabling map transitions, dungeon entrances, and fast travel mechanics throughout the game world.
/// </summary>
public struct PortalComponent(ushort x, ushort y)
{
    public ushort X = x;
    public ushort Y = y;
}