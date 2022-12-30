using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public struct AttributeComponent
    {
        public readonly int EntityId;
        public ushort Strength;
        public ushort Agility;
        public ushort Vitality;
        public ushort Spirit;
        public ushort Statpoints;

        public AttributeComponent(int entityId)
        {
            EntityId = entityId;
            Strength = 0;
            Agility = 0;
            Vitality = 0;
            Spirit = 0;
            Statpoints = 0;
        }
        public override int GetHashCode() => EntityId;
    }
}