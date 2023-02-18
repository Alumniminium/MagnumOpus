using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public readonly struct DestroyEndOfFrameComponent
    {
        public readonly int EntityId;
        public DestroyEndOfFrameComponent(int entityId) => EntityId = entityId;
        public override int GetHashCode() => EntityId;
    }

}