using System.Numerics;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
namespace MagnumOpus.Components.AI
{
    [Component]
    [Save]
    public struct BrainComponent
    {
        public BrainState State;
        public int TargetId;
        public Vector2 TargetPosition;
        public int SleepTicks;

        public BrainComponent()
        {
            State = BrainState.Idle;
            SleepTicks = 0;
        }
    }
}