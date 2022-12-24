using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public readonly struct DeathTagComponent
    {
        public readonly int EntityId;
        public readonly PixelEntity Killer;
        
        public DeathTagComponent(int entityId, in PixelEntity killer)
        {
            EntityId = entityId;
            Killer = killer;
        }
        public override int GetHashCode() => EntityId;
    }
}