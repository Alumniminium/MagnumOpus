using System.Numerics;
using MagnumOpus.Components;
using NttECS.ECS;

namespace MagnumOpus.AOGP
{
    /// <summary>
    /// Advanced GOAP planner using A* search algorithm for optimal action sequences.
    /// Provides intelligent planning with state prediction and heuristic evaluation.
    /// </summary>
    public static class GOAPPlanner
    {
        private const int MaxPlanningDepth = 10;
        private const int MaxNodesExpanded = 1000;

        /// <summary>
        /// Plans an optimal sequence of actions to achieve the given goal using A* search.
        /// </summary>
        /// <param name="ntt">The entity planning actions</param>
        /// <param name="availableActions">List of actions the entity can perform</param>
        /// <param name="goal">The goal to achieve</param>
        /// <returns>Ordered list of actions to achieve the goal, or empty if no plan found</returns>
        public static List<GOAPAction> Plan(in NTT ntt, List<GOAPAction> availableActions, GOAPGoal goal)
        {
            if (goal.IsGoalAchieved(ntt))
                return new List<GOAPAction>();

            var startState = new WorldState(ntt);
            var openSet = new PriorityQueue<PlanNode, float>();
            var closedSet = new HashSet<WorldState>();
            var nodeCount = 0;

            var startNode = new PlanNode(startState, new List<GOAPAction>(), 0, goal.CalculateHeuristic(ntt));
            openSet.Enqueue(startNode, startNode.TotalCost);

            while (openSet.Count > 0 && nodeCount < MaxNodesExpanded)
            {
                var currentNode = openSet.Dequeue();
                nodeCount++;

                if (closedSet.Contains(currentNode.State))
                    continue;

                closedSet.Add(currentNode.State);

                // Check if we've achieved the goal
                if (goal.IsGoalAchieved(currentNode.State, ntt))
                    return currentNode.ActionSequence;

                // Don't expand beyond max depth
                if (currentNode.ActionSequence.Count >= MaxPlanningDepth)
                    continue;

                // Expand available actions
                foreach (var action in availableActions)
                {
                    if (!action.CanExecute(currentNode.State, ntt))
                        continue;

                    var newState = action.PredictWorldState(currentNode.State, ntt);
                    var newActionSequence = new List<GOAPAction>(currentNode.ActionSequence) { action };
                    var newCost = currentNode.CostSoFar + action.CalculateCost(ntt);
                    var heuristic = goal.CalculateHeuristic(newState, ntt);

                    var newNode = new PlanNode(newState, newActionSequence, newCost, heuristic);
                    
                    if (!closedSet.Contains(newState))
                        openSet.Enqueue(newNode, newNode.TotalCost);
                }
            }

            // No plan found, return best partial plan or fallback
            return GetFallbackPlan(ntt, availableActions, goal);
        }

        /// <summary>
        /// Provides a fallback plan when A* search fails to find a complete solution.
        /// Uses greedy selection for immediate action.
        /// </summary>
        private static List<GOAPAction> GetFallbackPlan(in NTT ntt, List<GOAPAction> availableActions, GOAPGoal goal)
        {
            var plan = new List<GOAPAction>();
            GOAPAction? bestAction = null;
            var lowestCost = float.MaxValue;

            foreach (var action in availableActions)
            {
                if (!action.PreconditionsFulfilled(ntt))
                    continue;

                var totalCost = action.CalculateCost(ntt) + goal.CalculateHeuristic(ntt);
                if (totalCost < lowestCost)
                {
                    bestAction = action;
                    lowestCost = totalCost;
                }
            }

            if (bestAction != null)
                plan.Add(bestAction);

            return plan;
        }
    }

    /// <summary>
    /// Represents a node in the A* search tree for action planning.
    /// </summary>
    public class PlanNode
    {
        public WorldState State { get; }
        public List<GOAPAction> ActionSequence { get; }
        public float CostSoFar { get; }
        public float Heuristic { get; }
        public float TotalCost => CostSoFar + Heuristic;

        public PlanNode(WorldState state, List<GOAPAction> actionSequence, float costSoFar, float heuristic)
        {
            State = state;
            ActionSequence = actionSequence;
            CostSoFar = costSoFar;
            Heuristic = heuristic;
        }
    }

    /// <summary>
    /// Represents the state of the world for planning purposes.
    /// Contains essential information for action precondition checking and state prediction.
    /// </summary>
    public class WorldState : IEquatable<WorldState>
    {
        public Vector2 Position { get; set; }
        public NTT Target { get; set; }
        public float DistanceToTarget { get; set; }
        public bool IsInCombat { get; set; }
        public bool CanAttack { get; set; }
        public bool NeedsToMove { get; set; }

        public WorldState(in NTT ntt)
        {
            if (ntt.Has<PositionComponent>())
                Position = ntt.Get<PositionComponent>().Position;

            if (ntt.Has<BrainComponent>())
            {
                var brain = ntt.Get<BrainComponent>();
                Target = brain.Target;
                
                if (Target != 0 && Target.Has<PositionComponent>())
                {
                    var targetPos = Target.Get<PositionComponent>().Position;
                    DistanceToTarget = Vector2.Distance(Position, targetPos);
                    CanAttack = DistanceToTarget <= 1.5f;
                    NeedsToMove = DistanceToTarget > 1.5f;
                }
            }

            IsInCombat = ntt.Has<CombatComponent>();
        }

        public WorldState(WorldState other)
        {
            Position = other.Position;
            Target = other.Target;
            DistanceToTarget = other.DistanceToTarget;
            IsInCombat = other.IsInCombat;
            CanAttack = other.CanAttack;
            NeedsToMove = other.NeedsToMove;
        }

        public bool Equals(WorldState? other)
        {
            if (other == null) return false;
            
            return Vector2.Distance(Position, other.Position) < 0.1f &&
                   Target == other.Target &&
                   Math.Abs(DistanceToTarget - other.DistanceToTarget) < 0.1f &&
                   IsInCombat == other.IsInCombat &&
                   CanAttack == other.CanAttack;
        }

        public override bool Equals(object? obj) => Equals(obj as WorldState);

        public override int GetHashCode()
        {
            return HashCode.Combine(
                ((int)Position.X) / 10 * 10, // Grid-based hashing for position tolerance
                ((int)Position.Y) / 10 * 10,
                Target.GetHashCode(),
                IsInCombat,
                CanAttack
            );
        }
    }
}