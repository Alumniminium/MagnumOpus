using MagnumOpus.ECS;
namespace MagnumOpus.Components.Entity
{
    [Component]
    [Save]
    public readonly struct PkPointComponent
    {
        public readonly byte Points;
        public readonly TimeSpan DecreaseTime;

        public PkPointComponent(byte points, TimeSpan decreaseTime)
        {
            Points = points;
            DecreaseTime = decreaseTime;
        }
    }
}