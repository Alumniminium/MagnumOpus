using MagnumOpus.ECS;

namespace MagnumOpus.Simulation.Components
{
    [Component]
    public struct DropResourceComponent
    {
        public readonly int EntityId;
        public byte Amount;

        public DropResourceComponent(int entityId, int amount)
        {
            EntityId = entityId;
            Amount = (byte)amount;
        }
        public override int GetHashCode()
        {
            return EntityId;
        }
    }
}