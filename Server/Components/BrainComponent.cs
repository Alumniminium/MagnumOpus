using System.Numerics;
using MagnumOpus.AOGP;
using MagnumOpus.Enums;
using NttECS.ECS;
namespace MagnumOpus.Components
{
    [Component(SaveEnabled: true)]
    /// <summary>
    /// AI brain component implementing Goal-Oriented Action Planning (GOAP) for intelligent 
    /// entity behavior. Manages AI states (Idle, Sleeping, WakingUp, Approaching, Attacking), 
    /// target tracking, action plans, and sleep cycles. Used by BasicAISystem and GuardAISystem 
    /// to control monster behavior patterns and decision-making processes.
    /// </summary>
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