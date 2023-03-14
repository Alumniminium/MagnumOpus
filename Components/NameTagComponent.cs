using System.Text.Json.Serialization;
using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    [Save]
    public struct NameTagComponent
    {
        public string Name;

        public NameTagComponent() => Name = "Unnamed NTT";
        public NameTagComponent(string Name) => this.Name = Name;
    }
}