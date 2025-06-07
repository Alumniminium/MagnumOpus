using System.Numerics;
using MagnumOpus.Enums;
using NttECS.ECS;

namespace MagnumOpus.Components
{
    [Component(SaveEnabled: true)]
    public struct PositionComponent(Vector2 position, int map)
    {
        public long ChangedTick = NttWorld.Tick;
        public Vector2 Position = position;
        public Direction Direction = Direction.North;
        public Vector2 LastPosition = position;
        public int Map = map;
    }
}