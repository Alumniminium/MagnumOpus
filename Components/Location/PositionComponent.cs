using System.Numerics;
using MagnumOpus.ECS;
using Newtonsoft.Json;

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

        public PositionComponent(Vector2 position, int map)
        {
            Position = position;
            LastPosition = position;
            ChangedTick = NttWorld.Tick;
            Map = map;
        }
    }
}