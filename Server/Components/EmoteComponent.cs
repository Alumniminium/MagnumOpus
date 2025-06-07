using MagnumOpus.Enums;
using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
/// <summary>
/// Character emote/animation component that manages visual character expressions and poses.
/// Contains the current emote state (Stand, Sit, Dance, etc.) with manual change tracking
/// for network synchronization. Processed by EmoteSystem for animation updates and WalkSystem
/// for emote clearing when movement begins. Default state is Stand emote.
/// </summary>
public struct EmoteComponent(Emote emote = Emote.Stand)
{
    public long ChangedTick = NttWorld.Tick;
    private Emote _emote = emote;

    public Emote Emote
    {
        readonly get => _emote;
        set
        {
            if (_emote != value)
            {
                _emote = value;
                ChangedTick = NttWorld.Tick;
            }
        }
    }
}