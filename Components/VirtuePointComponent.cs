using MagnumOpus.ECS;
namespace MagnumOpus.Components
{



    [Component(saveEnabled: true)]
    public struct VirtuePointComponent
    {
        public long Points;

        public VirtuePointComponent(long points) => Points = points;
    }
}