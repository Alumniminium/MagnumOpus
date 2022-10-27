using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct NameTagComponent
    {
        public readonly int EntityId;
        public string Name;

        public NameTagComponent(int entityId, string name)
        {
            EntityId = entityId;
            Name = name;
        }
        public override int GetHashCode()
        {
            return EntityId;
        }
    }
}