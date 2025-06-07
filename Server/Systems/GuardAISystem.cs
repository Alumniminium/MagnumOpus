using System.Numerics;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Squiggly;

namespace MagnumOpus.Systems
{
    /// <summary>
    /// Controls guard monsters that protect specific positions by attacking nearby non-guard monsters and returning to their post.
    /// </summary>
    public sealed class GuardAISystem : NttSystem<PositionComponent, ViewportComponent, GuardPositionComponent, BrainComponent>
    {
        /// <summary>
        /// Initializes the GuardAISystem with full multi-threaded processing capabilities.
        /// </summary>
        public GuardAISystem() : base("Guard AI", threads: Environment.ProcessorCount) { }
        
        /// <summary>
        /// Filters entities to only process guard monsters.
        /// </summary>
        /// <param name="ntt">Entity to check for guard AI processing eligibility</param>
        /// <returns>True if entity is a monster with guard behavior</returns>
        protected override bool MatchesFilter(in NTT ntt) => ntt.Type == EntityType.Monster && base.MatchesFilter(in ntt);

        /// <summary>
        /// Processes guard AI behavior including target detection, approaching, attacking, and returning to guard position.
        /// </summary>
        /// <param name="ntt">The guard monster entity</param>
        /// <param name="pos">Current position component</param>
        /// <param name="vwp">Viewport component for detecting nearby threats</param>
        /// <param name="grd">Guard position component containing the position to defend</param>
        /// <param name="brn">Brain component for AI state management</param>
        public override void Update(in NTT ntt, ref PositionComponent pos, ref ViewportComponent vwp, ref GuardPositionComponent grd, ref BrainComponent brn)
        {
            if (brn.State == BrainState.Idle)
                return;
            if (brn.State == BrainState.Sleeping)
            {
                brn.SleepTicks--;

                if (brn.SleepTicks > 0)
                    return;
                else
                    brn.State = BrainState.WakingUp;
            }

            if (brn.State == BrainState.WakingUp)
            {
                vwp.EntitiesVisible.Clear();
                Collections.SpatialHashes[pos.Map].GetVisibleEntities(ref vwp);

                var closestDistance = int.MaxValue;
                var closestEntity = default(NTT);

                foreach (var visibleEntity in vwp.EntitiesVisible)
                {
                    if (visibleEntity.Type != EntityType.Monster)
                        continue;

                    if (visibleEntity.Has<GuardPositionComponent>() || visibleEntity.Has<DeathTagComponent>())
                        continue;

                    ref readonly var targetPosition = ref visibleEntity.Get<PositionComponent>();

                    var distance = (int)Vector2.Distance(grd.Position, targetPosition.Position);
                    if (distance > 18 || distance > closestDistance)
                        continue;

                    closestDistance = distance;
                    closestEntity = visibleEntity;
                    if (IsLogging)
                        FConsole.WriteLine("{ntt} found {visibleEntity} distance {dist}", ntt, visibleEntity, distance);
                }

                if (closestEntity.Id != 0)
                {
                    ref readonly var targetPos = ref closestEntity.Get<PositionComponent>();
                    brn.Target = closestEntity;
                    brn.TargetPosition = targetPos.Position;
                    brn.State = BrainState.Approaching;
                }
            }

            if (brn.Target == 0)
            {
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
                if (!NttWorld.EntityExists(brn.Target))
                {
                    brn.Target = default;
                    return;
                }

                ref readonly var target = ref NttWorld.GetEntity(brn.Target);
                if (target.Has<DeathTagComponent>() || target.Type != EntityType.Monster)
                {
                    brn.Target = default;
                    return;
                }

                var distance = (int)Vector2.Distance(pos.Position, brn.TargetPosition);

                brn.State = distance > 1 ? BrainState.Approaching : BrainState.Attacking;
            }

            if (brn.State == BrainState.Approaching)
            {
                var direction = CoMath.GetRawDirection(brn.TargetPosition, pos.Position);

                var walkComponent = new WalkComponent(direction, true);
                ntt.Set(ref walkComponent);

                if (IsLogging)
                    FConsole.WriteLine("{ntt} walking towards {target}", ntt, brn.TargetPosition);
            }

            if (brn.State == BrainState.Attacking)
            {
                ref readonly var targetEntity = ref NttWorld.GetEntity(brn.Target);
                var attackComponent = new AttackComponent(in targetEntity, MsgInteractType.Physical);
                ntt.Set(ref attackComponent);
                if (IsLogging)
                    FConsole.WriteLine("{ntt} attacking {target}", ntt, targetEntity);
            }

            brn.State = BrainState.Sleeping;
            brn.SleepTicks = (int)(NttWorld.TargetTps * Random.Shared.NextSingle());
        }
    }
}
