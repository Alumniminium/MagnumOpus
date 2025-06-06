using System.Numerics;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.SourceGeneration;

namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public partial struct PositionComponent
    {
        public long ChangedTick = NttWorld.Tick;
        
        private Vector2 _position;
        private Direction _direction;
        
        // Not tracked - these change for different reasons  
        public Vector2 LastPosition;
        public int Map;

        public PositionComponent(Vector2 position, int map)
        {
            LastPosition = position;
            Map = map;
            Position = position;    // Uses generated property
            Direction = Direction.North;  // Uses generated property
        }
    }
}