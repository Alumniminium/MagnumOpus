using System.Text.Json.Serialization;
using MagnumOpus.ECS;

namespace MagnumOpus.Components.Entity
{
    [Component]
    [Save]
    public struct NameTagComponent
    {
        public string Name;
        public NameTagComponent(string Name) => this.Name = Name;
    }
}