using MagnumOpus.ECS;

namespace MagnumOpus.Simulation.Components
{
    [Component]
    public readonly struct ExpRewardComponent
    {
        public readonly int EntityId;
        public readonly uint Experience;

        public ExpRewardComponent(int entityId, uint experience)
        {
            EntityId = entityId;
            Experience = experience;
        }
        public override int GetHashCode()
        {
            return EntityId;
        }
    }
}