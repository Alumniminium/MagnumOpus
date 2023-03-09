using System.Numerics;
using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Components.AI
{
    [Component]
    public struct BrainComponent
    {
        public readonly int EntityId;
        public BrainState State;
        public int TargetId;
        public Vector2 TargetPosition;
        public int SleepTicks;


        public BrainComponent(int entityId)
        {
            EntityId = entityId;
            State = BrainState.Idle;
            SleepTicks = 0;
        }

        public override int GetHashCode() => EntityId;
    }
}