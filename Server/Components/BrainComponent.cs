using System.Numerics;
using MagnumOpus.AOGP;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct BrainComponent(params GOAPAction[] actions)
    {
        public BrainState State = BrainState.Idle;
        public NTT Target;
        public Vector2 TargetPosition = Vector2.Zero;
        public int SleepTicks = 0;
        public List<GOAPAction> Plan = [];
        public List<GOAPAction> AvailableActions = new List<GOAPAction>(actions);
    }
}