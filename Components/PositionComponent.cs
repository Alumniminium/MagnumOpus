using System.Numerics;
using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct PositionComponent
    {
        public long ChangedTick;
        public Vector2 Position;
        public Vector2 LastPosition;
        public Direction Direction;
        public int Map;

        public PositionComponent(Vector2 position, int map)
        {
            ChangedTick = NttWorld.Tick;
            Position = position;
            LastPosition = position;
            Map = map;
        }
    }
}