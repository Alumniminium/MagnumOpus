using MagnumOpus.Enums;
using NttECS.ECS;

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