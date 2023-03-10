using MagnumOpus.ECS;
using MagnumOpus.Enums;
namespace MagnumOpus.Components.Location
{
    [Component]
    public  struct WalkComponent
    {
        public  long ChangedTick;
        public  Direction Direction;
        public  bool IsRunning;

        public WalkComponent() => ChangedTick = NttWorld.Tick;
        public WalkComponent(byte direction, bool isRunning)
        {
            ChangedTick = NttWorld.Tick;
            Direction = (Direction)(direction % 8);
            IsRunning = isRunning;
        }
    }
}