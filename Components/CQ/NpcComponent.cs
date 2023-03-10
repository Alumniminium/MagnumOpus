using MagnumOpus.ECS;
namespace MagnumOpus.Components.CQ
{
    [Component]
    [Save]
    public readonly struct NpcComponent
    {
        public readonly ushort Base;
        public readonly ushort Type;
        public readonly ushort Sort;

        public NpcComponent(ushort baseId, ushort typeId, ushort sort)
        {
            Base = baseId;
            Type = typeId;
            Sort = sort;
        }
    }
}