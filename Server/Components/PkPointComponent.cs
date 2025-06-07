using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component(SaveEnabled: true)]
    public struct PkPointComponent(byte points, TimeSpan decreaseTime)
    {
        public long ChangedTick = NttWorld.Tick;
        public byte Points = points;
        public TimeSpan DecreaseTime = decreaseTime;
    }
}