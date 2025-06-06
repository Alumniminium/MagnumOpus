using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct RecordPointComponent(ushort x, ushort y, ushort map)
    {
        public ushort Map = map;
        public ushort X = x;
        public ushort Y = y;
    }
}