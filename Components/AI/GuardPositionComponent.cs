using System.Numerics;
using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component][Save]
    public readonly struct GuardPositionComponent
    {
        public readonly int EntityId;
        public readonly Vector2 Position;

        public GuardPositionComponent(int entityId, Vector2 pos)
        {
            EntityId = entityId;
            Position = pos;
        }
        
        public override int GetHashCode() => EntityId;        
    }
}