using MagnumOpus.IO;
using MagnumOpus.AOGP;
using MagnumOpus.AOGP.Goals;
using MagnumOpus.Components;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using NttECS.ECS;

namespace MagnumOpus.Systems;

public sealed class BasicAISystem : NttSystem<PositionComponent, ViewportComponent, BrainComponent>
{
    public BasicAISystem() : base("Basic AI", threads: 1, log: false) { }

    protected override bool MatchesFilter(in NTT ntt) => ntt.IsMonster(guardsAreMonsters: true) && !ntt.Has<DeathTagComponent>() && !ntt.Has<GuardPositionComponent>() && base.MatchesFilter(in ntt);

    // Manages AI behavior for monsters using Goal-Oriented Action Planning (GOAP). The system handles
    // different brain states (idle, sleeping, waking up) and performs target acquisition by scanning
    // for visible players. When a target is found, it creates a plan using GOAP and executes actions
    // one at a time. After each action, the monster sleeps for a random duration (1-2 seconds).
    public override void Update(in NTT ntt, ref PositionComponent pos, ref ViewportComponent vwp, ref BrainComponent brn)
    {
        // Skip processing for idle monsters
        if (brn.State == BrainState.Idle)
            return;

        // Handle sleep state - count down sleep timer
        if (brn.State == BrainState.Sleeping)
        {
            brn.SleepTicks--;
            if (brn.SleepTicks > 0)
                return;
        }

        // Handle wake up - request viewport update to see surroundings
        if (brn.State == BrainState.WakingUp)
        {
            ntt.Set<ViewportUpdateTagComponent>();

            if (IsLogging)
                FConsole.WriteLine("Waking up {ntt} with {visibleCount} visible entities", ntt, vwp.EntitiesVisible.Count);
        }

        // Validate current target is still visible
        if (!vwp.EntitiesVisible.Contains(brn.Target))
            brn.Target = default;

        // Find a new target if we don't have one
        if (brn.Target == default)
        {
            // Scan for visible players to target
            brn.Target = vwp.Query<PlayerComponent>().Without<DeathTagComponent>().NearestTo(pos.Position);

            // No valid targets found - go idle
            if (brn.Target == 0)
            {
                brn.State = BrainState.Idle;
                return;
            }
        }

        // Execute GOAP planning and actions
        if (brn.Plan.Count == 0)
        {
            // Create new plan to defeat the target
            var goal = new DefeatEnemyGoal();
            brn.Plan = GOAPPlanner.Plan(ntt, brn.AvailableActions, goal);
        }
        else
        {
            // Execute next action in the plan, if the target is still alive
            if (!brn.Target.Has<DeathTagComponent>())
            {
                brn.Plan[0].Execute(ntt);
                brn.Plan.RemoveAt(0);
            }
            else
                brn.Plan.Clear();
        }

        // Put monster to sleep for 1-2 seconds after action
        brn.State = BrainState.Sleeping;
        brn.SleepTicks = (int)(NttWorld.TargetTps * (1 + Random.Shared.NextSingle()));
    }
}
