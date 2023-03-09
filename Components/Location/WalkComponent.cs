using MagnumOpus.ECS;
using MagnumOpus.Enums;
using Newtonsoft.Json;

namespace MagnumOpus.Components.Location
{
    [Component]
    public readonly struct WalkComponent
    {
        public readonly long ChangedTick;
        public readonly Direction Direction;
        public readonly bool IsRunning;

        [JsonConstructor]
        public WalkComponent(byte direction, bool isRunning)
        {
            ChangedTick = NttWorld.Tick;
            Direction = (Direction)(direction % 8);
            IsRunning = isRunning;
        }
    }
}