using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public partial struct PkPointComponent
    {
        public long ChangedTick = NttWorld.Tick;
        private byte _points;
        private TimeSpan _decreaseTime;

        public PkPointComponent() { }
        public PkPointComponent(byte points, TimeSpan decreaseTime)
        {
            Points = points;                // Uses generated property
            DecreaseTime = decreaseTime;    // Uses generated property
        }
    }
}