using MagnumOpus.ECS;
namespace MagnumOpus.Components.Entity
{
    [Component]
    [Save]
    public struct PkPointComponent
    {
        public byte Points;
        public TimeSpan DecreaseTime;

        public PkPointComponent(byte points, TimeSpan decreaseTime)
        {
            Points = points;
            DecreaseTime = decreaseTime;
        }
    }
}