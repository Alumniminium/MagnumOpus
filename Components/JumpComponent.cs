using System.Numerics;
using MagnumOpus.ECS;

namespace MagnumOpus.Simulation.Components
{
    [Component]
    public struct JumpComponent
    {
        public readonly int EntityId;
        public readonly uint CreatedTick;
        public readonly uint ChangedTick;
        public readonly Vector2 Position;
        public float Time;

        public JumpComponent(int entityId, ushort x, ushort y)
        {
            EntityId = entityId;
            ChangedTick = ConquerWorld.Tick;
            CreatedTick = ConquerWorld.Tick;
            Position = new Vector2(x, y);
            Time = 0;
        }

        public override int GetHashCode() => EntityId;
    }
}