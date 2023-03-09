using MagnumOpus.ECS;
using Newtonsoft.Json;

namespace MagnumOpus.Components.Entity
{
    [Component]
    [Save]
    public struct VirtuePointComponent
    {
        public long Points;

        [JsonConstructor]
        public VirtuePointComponent(long points) => Points = points;
    }
}