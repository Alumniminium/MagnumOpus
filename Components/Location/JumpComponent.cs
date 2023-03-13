using System.Numerics;
using MagnumOpus.ECS;
namespace MagnumOpus.Components.Location
{
    [Component]
    public struct JumpComponent
    {
        public long CreatedTick;
        public Vector2 Position;
        public float Time;

        public JumpComponent(ushort x, ushort y)
        {
            CreatedTick = NttWorld.Tick;
            Position = new Vector2(x, y);
            Time = 0;
        }
    }
}