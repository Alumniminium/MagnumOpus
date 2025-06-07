using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
/// <summary>
/// Player virtue/karma points component tracking moral alignment and reputation. Contains
/// virtue point total with change tracking for network synchronization. Higher values indicate
/// positive karma from helpful actions, while negative values indicate negative karma from
/// harmful actions. Currently defined but not actively processed by any systems - represents
/// planned morality system for reputation, NPC interactions, and gameplay restrictions.
/// </summary>
public struct VirtuePointComponent(long points)
{
    public long ChangedTick = NttWorld.Tick;
    public long Points = points;
}