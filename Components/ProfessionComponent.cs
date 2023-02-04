using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Components
{
    [Component]
    public struct ClassComponent
    {
        public readonly int EntityId;
        public ClasseName Class;

        public ClassComponent(int entityId, ClasseName profession)
        {
            EntityId = entityId;
            Class = profession;
        }
        public override int GetHashCode()
        {
            return EntityId;
        }
    }
}