using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct NameTagComponent
    {
        public string Name;

        public NameTagComponent() => Name = "Unnamed NTT";
        public NameTagComponent(string Name) => this.Name = Name;
    }
}