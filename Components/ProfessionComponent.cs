using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Simulation.Components
{
    [Component]
    public struct ProfessionComponent
    {
        public readonly int EntityId;
        public readonly ClasseName Class;

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