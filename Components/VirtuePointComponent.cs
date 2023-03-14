using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component]
    [Save]
    public struct VirtuePointComponent
    {
        public long Points;

        public VirtuePointComponent(long points) => Points = points;
    }
}