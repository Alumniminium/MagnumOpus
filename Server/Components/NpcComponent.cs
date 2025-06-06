using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct NpcComponent(ushort baseId, ushort typeId, ushort sort)
    {
        public ushort Base = baseId;
        public ushort Type = typeId;
        public ushort Sort = sort;
    }
}