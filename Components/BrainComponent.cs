using System.Numerics;
using MagnumOpus.AOGP;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct BrainComponent
    {
        public BrainState State;
        public int TargetId;
        public Vector2 TargetPosition;
        public int SleepTicks;
        public List<GOAPAction> Plan;

        public BrainComponent()
        {
            Plan = new List<GOAPAction>();
            State = BrainState.Idle;
            SleepTicks = 0;
        }
    }
}