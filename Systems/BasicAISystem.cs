using MagnumOpus.AOGP;
using MagnumOpus.AOGP.Goals;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.Systems
{
    public sealed class BasicAISystem : NttSystem<PositionComponent, ViewportComponent, BrainComponent>
    {
        public BasicAISystem() : base("Basic AI", threads: Environment.ProcessorCount) { }
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
                ntt.Set<ViewportUpdateTagComponent>();

                if (IsLogging)
                    Logger.Debug("Waking up {ntt} with {visibleCount} visible entities", ntt, vwp.EntitiesVisible.Count);
            }

            if (brn.Target == 0)
            {
                foreach (var b in vwp.EntitiesVisible)
                {
                    if (b.Type != EntityType.Player)
                        continue;

                    if (b.Has<DeathTagComponent>())
                        continue;

                    brn.Target = b;
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
