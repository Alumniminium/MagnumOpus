using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
/// <summary>
/// Entity teleportation component that schedules instant transportation to a target location.
/// Contains destination coordinates and map ID for teleportation requests. Processed by
/// TeleportSystem to update entity position, handle map transitions, send location packets
/// to clients, and trigger viewport refresh for visibility updates. Component is automatically
/// removed after teleportation completes. Essential for instant travel, portals, and magic spells.
/// </summary>
public struct TeleportComponent(ushort x, ushort y, ushort map)
{
    public ushort Map = map;
    public ushort X = x;
    public ushort Y = y;
}