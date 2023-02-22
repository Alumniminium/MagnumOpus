using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component][Save]
    public struct NameTagComponent
    {
        public readonly int EntityId;
        public string Name = String.Empty;

        public NameTagComponent(int entityId, string name)
        {
            EntityId = entityId;
            Name = name;
        }
        public override int GetHashCode() => EntityId;
    }
}