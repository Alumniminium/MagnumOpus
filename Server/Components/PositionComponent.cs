using System.Numerics;
using MagnumOpus.Enums;
using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
/// <summary>
/// Core spatial component managing entity location, orientation, and map placement. Contains
/// current position, facing direction, previous position for movement tracking, and map ID.
/// Extensively used throughout the system: WalkSystem for movement, SpatialHashSystem for
/// spatial indexing, ViewportSystem for visibility, AI systems for pathfinding, TeleportSystem
/// for position changes, and many others. Critical component for all spatial game mechanics.
/// </summary>
public struct PositionComponent(Vector2 position, int map)
{
    public long ChangedTick = NttWorld.Tick;
    public Vector2 Position = position;
    public Direction Direction = Direction.North;
    public Vector2 LastPosition = position;
    public int Map = map;
}