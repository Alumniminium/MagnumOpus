using System.Numerics;
using MagnumOpus.ECS;
namespace MagnumOpus.Components
{



    [Component(saveEnabled: true)]
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