using System.Numerics;
using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component]
    public readonly struct GuardComponent
    {
        public readonly int EntityId;
        public readonly Vector2 Position;

        public GuardComponent(int entityId, Vector2 pos)
        {
            EntityId = entityId;
            Position = pos;
        }
        
        public override int GetHashCode() => EntityId;        
    }
}