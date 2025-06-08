using NttECS.ECS;

namespace MagnumOpus.AOGP;

public abstract class GOAPGoal
{
    public abstract bool IsGoalAchieved(in NTT ntt);

    public virtual bool IsGoalAchieved(WorldState state, in NTT ntt)
    {
        return IsGoalAchieved(ntt);
    }

    public virtual float CalculateHeuristic(in NTT ntt)
    {
        return IsGoalAchieved(ntt) ? 0f : 1f;
    }

    public virtual float CalculateHeuristic(WorldState state, in NTT ntt)
    {
        return IsGoalAchieved(state, ntt) ? 0f : 1f;
    }
}