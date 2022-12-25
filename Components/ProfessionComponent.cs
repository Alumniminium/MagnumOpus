using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Components
{
    [Component]
    public struct ProfessionComponent
    {
        public readonly int EntityId;
        public ClasseName Class;

        public ProfessionComponent(int entityId, ClasseName profession)
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