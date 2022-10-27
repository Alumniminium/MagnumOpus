using System.Numerics;
using MagnumOpus.ECS;

namespace MagnumOpus.Simulation.Components
{
    [Component]
    public struct PositionComponent
    {
        public readonly int EntityId;
        public uint ChangedTick;

        public Vector2 Position;
        public Vector2 LastPosition;
        public int Map;

        public PositionComponent(int nttId, Vector2 position, int map)
        {
            EntityId = nttId;
            Position = position;
            LastPosition = position;
            ChangedTick = ConquerWorld.Tick;
            Map = map;
        }

        public override int GetHashCode() => EntityId;
    }
}