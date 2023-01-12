using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public readonly struct ReviveComponent
    {
        public readonly int EntityId;
        public readonly long ReviveTick;
        public ReviveComponent(int id, uint seconds)
        {
            EntityId = id;
            ReviveTick = PixelWorld.Tick + (seconds * PixelWorld.TargetTps);
        }

        public override int GetHashCode() => EntityId;
    }
}