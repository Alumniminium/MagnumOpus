using MagnumOpus.Components;
using MagnumOpus.ECS;

namespace MagnumOpus.AOGP.Goals
{
    public class ProtectAreaGoal : GOAPGoal
    {
        public override bool IsGoalAchieved(in NTT ntt)
        {
            ref readonly var brn = ref ntt.Get<BrainComponent>();
            return brn.Target == default;
        }
    }
}
