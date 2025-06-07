using System.Numerics;
using NttECS.ECS;
namespace MagnumOpus.Components;

[Component]
/// <summary>
/// Transient jump animation component for entities performing jump movements. Contains target
/// position coordinates, creation timestamp, and animation time tracking. Not saved to database
/// (no SaveEnabled). Processed by JumpSystem to handle jump animations, movement interpolation,
/// and cleanup when jump completes. Used in DeathSystem for death animation effects.
/// </summary>
public struct JumpComponent(ushort x, ushort y)
{
    public long CreatedTick = NttWorld.Tick;
    public Vector2 Position = new Vector2(x, y);
    public float Time = 0;
}