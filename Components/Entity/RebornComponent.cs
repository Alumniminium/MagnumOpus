using MagnumOpus.ECS;
namespace MagnumOpus.Components.Entity
{
    [Component]
    [Save]
    public struct RebornComponent
    {
        public long ChangedTick;
        public byte Count;

        public RebornComponent() => ChangedTick = NttWorld.Tick;
    }
}