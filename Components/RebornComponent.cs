using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct RebornComponent
    {
        public long ChangedTick;
        public byte Count;

        public RebornComponent() => ChangedTick = NttWorld.Tick;
    }
}