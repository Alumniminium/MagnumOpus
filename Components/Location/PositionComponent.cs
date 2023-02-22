using System.Numerics;
using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component][Save]
    public struct PositionComponent
    {
        public readonly int EntityId;
        public long ChangedTick;

        public Vector2 Position;
        public Vector2 LastPosition;
        public int Map;

        public PositionComponent(int nttId, Vector2 position, int map)
        {
            EntityId = nttId;
            Position = position;
            LastPosition = position;
            ChangedTick = NttWorld.Tick;
            Map = map;
        }

        public override int GetHashCode() => EntityId;
    }
}