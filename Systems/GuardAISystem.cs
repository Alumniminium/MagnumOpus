using System.Numerics;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class GuardAISystem : NttSystem<PositionComponent, ViewportComponent, GuardPositionComponent, BrainComponent>
    {
        public GuardAISystem() : base("Guard AI", threads: Environment.ProcessorCount) { }
        protected override bool MatchesFilter(in NTT ntt) => ntt.Type == EntityType.Monster && base.MatchesFilter(in ntt);

        public override void Update(in NTT ntt, ref PositionComponent pos, ref ViewportComponent vwp, ref GuardPositionComponent grd, ref BrainComponent brn)
        {
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
                Game.Grids[pos.Map].GetVisibleEntities(ref vwp);
                var closestDistance = int.MaxValue;
                var closestEntity = default(NTT);

                for (var i = 0; i < vwp.EntitiesVisible.Count; i++)
                {
                    var b = vwp.EntitiesVisible[i];

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
                }

                if (closestEntity.Id != 0)
                {
                    ref readonly var tpos = ref closestEntity.Get<PositionComponent>();
                    brn.TargetId = closestEntity.Id;
                    brn.TargetPosition = tpos.Position;
                    brn.State = BrainState.Approaching;
                }
            }

            if (brn.TargetId == 0)
            {
                if (pos.Position != grd.Position)
                {
                    brn.TargetPosition = grd.Position;
                    brn.State = BrainState.Approaching;
                }
            }
            else
            {
                if (!NttWorld.EntityExists(brn.TargetId))
                {
                    brn.TargetId = 0;
                    return;
                }

                ref readonly var _target = ref NttWorld.GetEntity(brn.TargetId);
                if(_target.Has<DeathTagComponent>())
                {
                    brn.TargetId = 0;
                    return;
                }

                var distance = (int)Vector2.Distance(pos.Position, brn.TargetPosition);

                if (distance > 1)
                    brn.State = BrainState.Approaching;
                else
                    brn.State = BrainState.Attacking;
            }

            if (brn.State == BrainState.Approaching)
            {
                var dir = CoMath.GetRawDirection(brn.TargetPosition, pos.Position);

                var wlk = new WalkComponent(ntt.Id, dir, true);
                ntt.Set(ref wlk);
            }

            if (brn.State == BrainState.Attacking)
            {
                ref readonly var _target = ref NttWorld.GetEntity(brn.TargetId);
                var atk = new AttackComponent(ntt.Id, in _target, MsgInteractType.Physical);
                ntt.Set(ref atk);
            }

            brn.State = BrainState.Sleeping;
            brn.SleepTicks = (int)(NttWorld.TargetTps * 0.33f);
        }
    }
}
