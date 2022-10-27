using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct TransformationComponent
    {
        public readonly int EntityId;
        public readonly uint Look;

        public TransformationComponent(int entityId, uint look)
        {
            EntityId = entityId;
            Look = look;
        }
        public override int GetHashCode()
        {
            return EntityId;
        }
    }
}