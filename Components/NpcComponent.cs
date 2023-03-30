using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct NpcComponent
    {
        public ushort Base;
        public ushort Type;
        public ushort Sort;

        public NpcComponent(ushort baseId, ushort typeId, ushort sort)
        {
            Base = baseId;
            Type = typeId;
            Sort = sort;
        }
    }
}