using System.Numerics;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Squiggly;

namespace MagnumOpus.Systems
{
    public sealed class GuardAISystem : NttSystem<PositionComponent, ViewportComponent, GuardPositionComponent, BrainComponent>
    {
        public GuardAISystem() : base("Guard AI", threads: Environment.ProcessorCount) { }
        protected override bool MatchesFilter(in NTT ntt) => ntt.Type == EntityType.Monster && base.MatchesFilter(in ntt);

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

                foreach (var b in vwp.EntitiesVisible)
                {
                    if (b.Type != EntityType.Monster)
                        continue;

                    if (b.Has<GuardPositionComponent>() || b.Has<DeathTagComponent>())
                        continue;

                    ref readonly var targetPos = ref b.Get<PositionComponent>();

                    var distance = (int)Vector2.Distance(grd.Position, targetPos.Position);
                    if (distance > 18 || distance > closestDistance)
                        continue;

                    closestDistance = distance;
                    closestEntity = b;
                    if (IsLogging)
                        FConsole.WriteLine("{ntt} found {b} distance {dist}", ntt, b, distance);
                }

                if (closestEntity.Id != 0)
                {
                    ref readonly var tpos = ref closestEntity.Get<PositionComponent>();
                    brn.Target = closestEntity;
                    brn.TargetPosition = tpos.Position;
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
                var dir = CoMath.GetRawDirection(brn.TargetPosition, pos.Position);

                var wlk = new WalkComponent(dir, true);
                ntt.Set(ref wlk);

                if (IsLogging)
                    FConsole.WriteLine("{ntt} walking towards {target}", ntt, brn.TargetPosition);
            }

            if (brn.State == BrainState.Attacking)
            {
                ref readonly var _target = ref NttWorld.GetEntity(brn.Target);
                var atk = new AttackComponent(in _target, MsgInteractType.Physical);
                ntt.Set(ref atk);
                if (IsLogging)
                    FConsole.WriteLine("{ntt} attacking {target}", ntt, _target);
            }

            brn.State = BrainState.Sleeping;
            brn.SleepTicks = (int)(NttWorld.TargetTps * Random.Shared.NextSingle());
        }
    }
}
