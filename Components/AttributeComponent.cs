using MagnumOpus.ECS;

namespace MagnumOpus.Simulation.Components
{
    [Component]
    public struct AttributeComponent
    {
        public readonly int EntityId;
        public uint ChangedTick;
        public ushort Strength;
        public ushort Agility;
        public ushort Vitality;
        public ushort Spirit;
        public ushort Statpoints;

        public AttributeComponent(int entityId)
        {
            EntityId = entityId;
            ChangedTick = 0;
            Strength = 0;
            Agility = 0;
            Vitality = 0;
            Spirit = 0;
            Statpoints = 0;
        }
        public override int GetHashCode()
        {
            return EntityId;
        }
    }
}