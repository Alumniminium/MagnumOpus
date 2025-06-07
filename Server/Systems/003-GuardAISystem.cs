using System.Numerics;
using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Squiggly;
using NttECS.ECS;

namespace MagnumOpus.Systems
{
    public sealed class GuardAISystem : NttSystem<PositionComponent, ViewportComponent, GuardPositionComponent, BrainComponent>
    {
        public GuardAISystem() : base("Guard AI", threads: 1, log: false) { }

        protected override bool MatchesFilter(in NTT ntt) => ntt.IsMonster() && base.MatchesFilter(in ntt);

        // Controls guard monsters that protect specific positions by attacking nearby non-guard
        // monsters and returning to their post. Guards scan for threats within 18 units of their
        // guard position, approach and attack the closest target, then return to their designated
        // position when no threats remain. Uses brain states to manage behavior transitions.
        public override void Update(in NTT ntt, ref PositionComponent pos, ref ViewportComponent vwp, ref GuardPositionComponent grd, ref BrainComponent brn)
        {
            // === HANDLE BRAIN STATE TRANSITIONS ===
            // Skip processing for idle guards
            if (brn.State == BrainState.Idle)
                return;

            // Handle sleep state - count down sleep timer
            if (brn.State == BrainState.Sleeping)
            {
                brn.SleepTicks--;
                if (brn.SleepTicks > 0)
                    return;
                
                brn.State = BrainState.WakingUp;
            }

            // === THREAT DETECTION AND TARGET ACQUISITION ===
            if (brn.State == BrainState.WakingUp)
            {
                // Refresh viewport to see current surroundings
                vwp.EntitiesVisible.Clear();
                Collections.SpatialHashes[pos.Map].GetVisibleEntities(ref vwp);

                var closestDistance = int.MaxValue;
                var closestThreat = default(NTT);

                // Scan for threats within guard range (18 units from guard position)
                foreach (var visibleEntity in vwp.EntitiesVisible)
                {
                    // Skip other guards and dead entities
                    if (visibleEntity.Has<GuardPositionComponent>() || visibleEntity.Has<DeathTagComponent>())
                        continue;

                    ref readonly var entityPosition = ref visibleEntity.Get<PositionComponent>();
                    var distanceFromGuardPost = (int)Vector2.Distance(grd.Position, entityPosition.Position);
                    
                    // Only consider threats within guard range and closer than current target
                    if (distanceFromGuardPost > 18 || distanceFromGuardPost > closestDistance)
                        continue;

                    closestDistance = distanceFromGuardPost;
                    closestThreat = visibleEntity;
                    
                    if (IsLogging)
                        FConsole.WriteLine("{ntt} detected threat {threat} at distance {dist} from guard post", 
                            ntt, visibleEntity, distanceFromGuardPost);
                }

                // Set target if threat found
                if (closestThreat.Id != 0)
                {
                    ref readonly var targetPos = ref closestThreat.Get<PositionComponent>();
                    brn.Target = closestThreat;
                    brn.TargetPosition = targetPos.Position;
                    brn.State = BrainState.Approaching;
                }
            }

            // === TARGET VALIDATION AND BEHAVIOR DECISION ===
            if (brn.Target == 0)
            {
                // No target - return to guard position if not already there
                if (pos.Position != grd.Position)
                {
                    brn.TargetPosition = grd.Position;
                    brn.State = BrainState.Approaching;
                }
                else
                {
                    brn.State = BrainState.Idle;
                }
            }
            else
            {
                // Validate current target still exists and is alive
                if (!NttWorld.EntityExists(brn.Target))
                {
                    brn.Target = default;
                    return;
                }

                ref readonly var target = ref NttWorld.GetEntity(brn.Target);
                if (target.Has<DeathTagComponent>())
                {
                    brn.Target = default;
                    return;
                }

                // Determine action based on distance to target
                var distanceToTarget = (int)Vector2.Distance(pos.Position, brn.TargetPosition);
                brn.State = distanceToTarget > 1 ? BrainState.Approaching : BrainState.Attacking;
            }

            // === EXECUTE ACTIONS BASED ON STATE ===
            if (brn.State == BrainState.Approaching)
            {
                // Move towards target position
                var direction = CoMath.GetRawDirection(brn.TargetPosition, pos.Position);
                var walkComponent = new WalkComponent(direction, true);
                ntt.Set(ref walkComponent);

                if (IsLogging)
                    FConsole.WriteLine("{ntt} approaching target at {pos}", ntt, brn.TargetPosition);
            }
            else if (brn.State == BrainState.Attacking)
            {
                // Attack the target
                ref readonly var targetEntity = ref NttWorld.GetEntity(brn.Target);
                var attackComponent = new AttackComponent(in targetEntity, MsgInteractType.Physical);
                ntt.Set(ref attackComponent);
                
                if (IsLogging)
                    FConsole.WriteLine("{ntt} attacking target {target}", ntt, targetEntity);
            }

            // Sleep for random duration (0-1 seconds) before next action
            brn.State = BrainState.Sleeping;
            brn.SleepTicks = (int)(NttWorld.TargetTps * Random.Shared.NextSingle());
        }
    }
}
