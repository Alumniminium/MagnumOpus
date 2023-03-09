using MagnumOpus.ECS;

namespace MagnumOpus.Components.CQ
{
    [Component]
    [Save]
    public readonly struct NpcComponent
    {
        public readonly int EntityId;
        public readonly ushort Base;
        public readonly ushort Type;
        public readonly ushort Sort;

        public NpcComponent(int entityId, ushort baseId, ushort typeId, ushort sort)
        {
            EntityId = entityId;
            Base = baseId;
            Type = typeId;
            Sort = sort;
        }
        public override int GetHashCode()
        {
            return EntityId;
        }
    }
}