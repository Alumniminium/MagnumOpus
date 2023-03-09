using System.Numerics;
using MagnumOpus.ECS;
using Newtonsoft.Json;

namespace MagnumOpus.Components.Location
{
    [Component]
    public struct JumpComponent
    {
        public readonly long CreatedTick;
        public readonly long ChangedTick;
        public readonly Vector2 Position;
        public float Time;

        [JsonConstructor]
        public JumpComponent(ushort x, ushort y)
        {
            ChangedTick = NttWorld.Tick;
            CreatedTick = NttWorld.Tick;
            Position = new Vector2(x, y);
            Time = 0;
        }
    }
}