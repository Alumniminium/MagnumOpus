using MagnumOpus.ECS;

namespace MagnumOpus.Components.Entity
{
    [Component]
    [Save]
    public struct NameTagComponent
    {
        public readonly int EntityId;
        public string Name = string.Empty;

        public NameTagComponent(int entityId, string name)
        {
            EntityId = entityId;
            Name = name;
        }
        public override int GetHashCode() => EntityId;
    }
}