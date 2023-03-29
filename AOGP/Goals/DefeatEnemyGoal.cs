using MagnumOpus.Components;
using MagnumOpus.ECS;

namespace MagnumOpus.AOGP.Goals
{
    public class DefeatEnemyGoal : GOAPGoal
    {
        public override bool IsGoalAchieved(in NTT ntt)
        {
            ref readonly var brn = ref ntt.Get<BrainComponent>();

            if (brn.TargetId == 0)
                return false;

            ref readonly var target = ref NttWorld.GetEntity(brn.TargetId);

            return target.Has<DeathTagComponent>();
        }
    }
}