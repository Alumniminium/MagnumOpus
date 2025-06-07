using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
/// <summary>
/// Player-Kill (PK) points component tracking player aggressive actions and reputation. Contains
/// PK point count and decay timer for gradual reputation recovery. Change tracking enables
/// network synchronization for PK status updates. Currently defined but not actively processed
/// by any systems - represents planned PvP reputation system for tracking player aggression
/// and applying penalties or restrictions based on PK behavior.
/// </summary>
public struct PkPointComponent(byte points, TimeSpan decreaseTime)
{
    public long ChangedTick = NttWorld.Tick;
    public byte Points = points;
    public TimeSpan DecreaseTime = decreaseTime;
}