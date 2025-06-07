using NttECS.ECS;

namespace MagnumOpus.Components;

[Component(SaveEnabled: true)]
public struct NameTagComponent
{
    public string Name;

    public NameTagComponent() => Name = "Unnamed NTT";
    public NameTagComponent(string Name) => this.Name = Name;
}