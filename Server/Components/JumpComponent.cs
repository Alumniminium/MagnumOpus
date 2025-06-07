using System.Numerics;
using NttECS.ECS;
namespace MagnumOpus.Components
{
    [Component]
    public struct JumpComponent(ushort x, ushort y)
    {
        public long CreatedTick = NttWorld.Tick;
        public Vector2 Position = new Vector2(x, y);
        public float Time = 0;
    }
}