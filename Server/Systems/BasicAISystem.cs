using MagnumOpus.IO;
using MagnumOpus.AOGP;
using MagnumOpus.AOGP.Goals;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using NttECS.ECS;

namespace MagnumOpus.Systems
{
    /// <summary>
    /// Handles basic AI behavior for monsters using GOAP (Goal-Oriented Action Planning).
    /// Manages target acquisition, action planning, and execution cycles for non-guard monsters.
    /// </summary>
    public sealed class BasicAISystem : NttSystem<PositionComponent, ViewportComponent, BrainComponent>
    {
        /// <summary>
        /// Initializes the BasicAISystem with full multi-threaded processing capabilities.
        /// </summary>
        public BasicAISystem() : base("Basic AI", threads: 1) { }

        /// <summary>
        /// Filters entities to only process living monsters that are not guards.
        /// </summary>
        /// <param name="ntt">Entity to check for AI processing eligibility</param>
        /// <returns>True if entity is a living monster without guard behavior</returns>
        protected override bool MatchesFilter(in NTT ntt) => ntt.IsMonster() && !ntt.Has<DeathTagComponent>() && !ntt.Has<GuardPositionComponent>() && base.MatchesFilter(in ntt);

        /// <summary>
        /// Processes AI behavior including state management, target acquisition, and action planning.
        /// </summary>
        /// <param name="ntt">The monster entity being processed</param>
        /// <param name="pos">Position component for location-based decisions</param>
        /// <param name="vwp">Viewport component for target detection</param>
        /// <param name="brn">Brain component containing AI state and planning data</param>
        public override void Update(in NTT ntt, ref PositionComponent pos, ref ViewportComponent vwp, ref BrainComponent brn)
        {
            if (brn.State == BrainState.Idle)
                return;

            if (brn.State == BrainState.Sleeping)
            {
                brn.SleepTicks--;
                if (brn.SleepTicks > 0)
                    return;
            }

            if (brn.State == BrainState.WakingUp)
            {
                ntt.Set<ViewportUpdateTagComponent>();

                if (IsLogging)
                    FConsole.WriteLine("Waking up {ntt} with {visibleCount} visible entities", ntt, vwp.EntitiesVisible.Count);
            }

            if (!vwp.EntitiesVisible.Contains(brn.Target))
                brn.Target = default;

            if (brn.Target == 0)
            {
                foreach (var visibleEntity in vwp.EntitiesVisible)
                {
                    if (!visibleEntity.IsPlayer())
                        continue;

                    if (visibleEntity.Has<DeathTagComponent>())
                        continue;

                    brn.Target = visibleEntity;
                    break;
                }
                if (brn.Target == 0)
                {
                    brn.State = BrainState.Idle;
                    return;
                }
            }

            if (brn.Plan.Count == 0)
            {
                var goal = new DefeatEnemyGoal();
                brn.Plan = GOAPPlanner.Plan(ntt, brn.AvailableActions, goal);
            }
            else
            {
                brn.Plan[0].Execute(ntt);
                brn.Plan.RemoveAt(0);
            }

            brn.State = BrainState.Sleeping;
            brn.SleepTicks = (int)(NttWorld.TargetTps * (1 + Random.Shared.NextSingle()));
        }
    }
}
