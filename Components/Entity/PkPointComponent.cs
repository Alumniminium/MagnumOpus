using MagnumOpus.ECS;
using Newtonsoft.Json;

namespace MagnumOpus.Components.Entity
{
    [Component]
    [Save]
    public readonly struct PkPointComponent
    {
        public readonly byte Points;
        public readonly TimeSpan DecreaseTime;

        [JsonConstructor]
        public PkPointComponent(byte points, TimeSpan decreaseTime)
        {
            Points = points;
            DecreaseTime = decreaseTime;
        }
    }
}