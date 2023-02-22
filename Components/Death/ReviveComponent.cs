using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component][Save]
    public readonly struct ReviveComponent
    {
        public readonly int EntityId;
        public readonly long ReviveTick;
        public ReviveComponent(int id, uint seconds)
        {
            EntityId = id;
            ReviveTick = NttWorld.Tick + (seconds * NttWorld.TargetTps);
        }

        public override int GetHashCode() => EntityId;
    }
}