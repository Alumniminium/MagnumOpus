using System.Numerics;
using MagnumOpus.ECS;

namespace MagnumOpus.Components
{
    [Component][Save]
    public struct JumpComponent
    {
        public readonly int EntityId;
        public readonly long CreatedTick;
        public readonly long ChangedTick;
        public readonly Vector2 Position;
        public float Time;

        public JumpComponent(int entityId, ushort x, ushort y)
        {
            EntityId = entityId;
            ChangedTick = NttWorld.Tick;
            CreatedTick = NttWorld.Tick;
            Position = new Vector2(x, y);
            Time = 0;
        }

        public override int GetHashCode() => EntityId;
    }
}