using NttECS.ECS;

namespace MagnumOpus.Components;

[Component]
/// <summary>
/// Network latency tracking component that monitors client-server round-trip times. Contains
/// current ping measurement and last ping timestamp for connection quality assessment. Not
/// saved to database (no SaveEnabled). Currently defined but not actively processed by any
/// systems - represents planned network monitoring functionality for lag detection and
/// connection quality metrics.
/// </summary>
public struct PingComponent
{
    public int LastPing;
    public int Ping;
}