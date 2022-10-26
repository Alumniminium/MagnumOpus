using System.Numerics;
using MagnumOpus.ECS;

namespace MagnumOpus.Simulation.Components
{
    [Component]
    public readonly struct JumpComponent
    {
        public readonly int EntityId;
        public readonly uint ChangedTick;

        public readonly Vector2 Position;

        public JumpComponent(int entityId, ushort x, ushort y)
        {
            EntityId = entityId;
            ChangedTick = PixelWorld.Tick;
            Position = new Vector2(x, y);
        }

        public override int GetHashCode() => EntityId;
    }
}