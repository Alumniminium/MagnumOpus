using System.Numerics;
using MagnumOpus.AOGP;
using MagnumOpus.AOGP.Actions;
using MagnumOpus.AOGP.Goals;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Squiggly;

namespace MagnumOpus.Systems
{
    public sealed class BasicAISystem : NttSystem<PositionComponent, ViewportComponent, BrainComponent>
    {
        public BasicAISystem() : base("Basic AI", threads: 12) { }
        protected override bool MatchesFilter(in NTT ntt) => ntt.Type == EntityType.Monster && !ntt.Has<DeathTagComponent>() && !ntt.Has<GuardPositionComponent>() && base.MatchesFilter(in ntt);

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
                if (ntt.CreatedTick + NttWorld.TargetTps * 1.5 > Tick)
                    return;

                vwp.EntitiesVisible.Clear();
                Collections.SpatialHashs[pos.Map].GetVisibleEntities(ref vwp);

                if (IsLogging)
                    Logger.Debug("Waking up {ntt} with {visibleCount} visible entities", ntt, vwp.EntitiesVisible.Count);
            }

            if (brn.TargetId == 0)
                FindNewTarget(ref vwp, ref brn);

            if (brn.TargetId == 0)
            {
                brn.State = BrainState.Idle;
                return;
            }

            if (brn.Plan.Count == 0)
            {
                var availableActions = new List<GOAPAction>
                {
                    new ApproachAction(),
                    new AttackAction(),
                };
                var goal = new DefeatEnemyGoal();

                brn.Plan = GOAPPlanner.Plan(ntt, availableActions, goal);
            }
            else
            {
                brn.Plan[0].Execute(ntt);
                brn.Plan.RemoveAt(0);
            }

            brn.State = BrainState.Sleeping;
            brn.SleepTicks = (int)(NttWorld.TargetTps * (1 + Random.Shared.NextSingle()));
        }


        private static void FindNewTarget(ref ViewportComponent vwp, ref BrainComponent brn)
        {
            foreach (var kvp in vwp.EntitiesVisible)
            {
                var b = kvp.Value;
                if (b.Type != EntityType.Player)
                    continue;

                if (b.Has<DeathTagComponent>())
                    continue;

                brn.TargetId = b.Id;
                break;
            }
        }
    }
}
