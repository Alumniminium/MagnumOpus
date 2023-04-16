using MagnumOpus.ECS;

namespace MagnumOpus.AOGP
{
    public static class GOAPPlanner
    {
        public static List<GOAPAction> Plan(in NTT ntt, List<GOAPAction> availableActions, GOAPGoal goal)
        {
            var plan = new List<GOAPAction>();

            if (!goal.IsGoalAchieved(ntt))
            {
                GOAPAction? bestAction = null;
                var lowestCost = int.MaxValue;

                foreach (var action in availableActions)
                {
                    if (!action.PreconditionsFulfilled(ntt))
                        continue;

                    if (action.Cost < lowestCost)
                    {
                        bestAction = action;
                        lowestCost = action.Cost;
                    }
                }

                if (bestAction == null)
                    return plan;

                plan.Add(bestAction);
                bestAction.Execute(ntt);
            }
            return plan;
        }
    }
}