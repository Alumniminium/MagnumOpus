using MagnumOpus.Enums;
using NttECS.ECS;

namespace MagnumOpus.Components;

[Component]
/// <summary>
/// Entity movement component that triggers walking behavior in a specified direction. Contains
/// movement direction (0-7 for 8-directional movement) and running/walking speed flag. Not
/// saved to database (no SaveEnabled). Processed by WalkSystem to update entity position,
/// handle collision detection, clear emotes, update spatial systems, and synchronize movement
/// to clients. Essential for all entity movement, created by AI systems and player input.
/// </summary>
public struct WalkComponent(byte direction, bool isRunning)
{
    public long ChangedTick = NttWorld.Tick;
    public Direction Direction = (Direction)(direction % 8);
    public bool IsRunning = isRunning;
}