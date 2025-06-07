using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
/// <summary>
/// Entity name component storing the display name for players, NPCs, monsters, and other
/// named entities. Provides human-readable identification for entities throughout the game.
/// Used extensively for logging, debugging, network synchronization, and UI display. Default
/// name is "Unnamed NTT" for entities without explicit names. Essential for entity identification
/// and user interface presentation.
/// </summary>
public struct NameTagComponent
{
    public string Name;

    public NameTagComponent() => Name = "Unnamed NTT";
    public NameTagComponent(string Name) => this.Name = Name;
}