using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.SourceGeneration;

namespace MagnumOpus.Components
{
    [Component]
    public partial struct WalkComponent
    {
        public long ChangedTick = NttWorld.Tick;

        private Direction _direction;
        private bool _isRunning;

        public WalkComponent() { }

        public WalkComponent(byte direction, bool isRunning)
        {
            Direction = (Direction)(direction % 8);  // Uses generated property
            IsRunning = isRunning;                    // Uses generated property
        }
    }
}