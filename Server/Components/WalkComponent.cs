using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Components
{
    [Component]
    public struct WalkComponent(byte direction, bool isRunning)
    {
        public long ChangedTick = NttWorld.Tick;
        public Direction Direction = (Direction)(direction % 8);
        public bool IsRunning = isRunning;
    }
}