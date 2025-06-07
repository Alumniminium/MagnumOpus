using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
/// <summary>
/// Character reborn/reincarnation component tracking the number of character rebirths. Contains
/// reborn count with change tracking for network synchronization. Reborn characters retain
/// experience while resetting level for extended progression. Currently defined but not actively
/// processed by any systems - represents planned reincarnation system for high-level character
/// advancement, prestige progression, and extended end-game content.
/// </summary>
public struct RebornComponent(byte count = 0)
{
    public long ChangedTick = NttWorld.Tick;
    public byte Count = count;
}