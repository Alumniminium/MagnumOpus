using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct RecordPointComponent
    {
        public ushort Map;
        public ushort X;
        public ushort Y;

        public RecordPointComponent(ushort x, ushort y, ushort map)
        {
            X = x;
            Y = y;
            Map = map;
        }
    }
}