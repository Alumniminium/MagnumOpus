using System.Numerics;
using MagnumOpus.ECS;
namespace MagnumOpus.Components.Location
{
    [Component]
    [Save]
    public struct PositionComponent
    {
        public long ChangedTick;

        public Vector2 Position;
        public Vector2 LastPosition;
        public int Map;

        public PositionComponent() => ChangedTick = NttWorld.Tick;
        public PositionComponent(Vector2 position, int map)
        {
            Position = position;
            LastPosition = position;
            ChangedTick = NttWorld.Tick;
            Map = map;
        }
    }
}