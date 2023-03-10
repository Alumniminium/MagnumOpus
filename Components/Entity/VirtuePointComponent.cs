using MagnumOpus.ECS;
namespace MagnumOpus.Components.Entity
{
    [Component]
    [Save]
    public struct VirtuePointComponent
    {
        public long Points;

        public VirtuePointComponent(long points) => Points = points;
    }
}