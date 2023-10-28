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
        public NTT Target;
        public Vector2 TargetPosition;
        public int SleepTicks;
        public List<GOAPAction> Plan;
        public List<GOAPAction> AvailableActions;

        public BrainComponent(params GOAPAction[] actions)
        {
            Plan = [];
            State = BrainState.Idle;
            SleepTicks = 0;
            TargetPosition = Vector2.Zero;
            AvailableActions = new List<GOAPAction>(actions);
        }
    }
}