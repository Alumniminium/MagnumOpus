using MagnumOpus.ECS;

namespace MagnumOpus.AOGP
{
    public abstract class GOAPGoal
    {
        public abstract bool IsGoalAchieved(in NTT ntt);
    }
}