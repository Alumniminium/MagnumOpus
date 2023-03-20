using System.Numerics;
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

            ref readonly var target = ref NttWorld.GetEntity(brn.TargetId);
            ref readonly var targetPos = ref target.Get<PositionComponent>();

            if (target.Has<DeathTagComponent>())
            {
                brn.TargetId = 0;
                return;
            }

            var distance = (int)Vector2.Distance(pos.Position, targetPos.Position);
            if (distance > 16)
            {
                brn.TargetId = 0;
                brn.State = BrainState.Idle;
                if (IsLogging)
                    Logger.Debug("{Entity} target {target} out of range", ntt, target);
                return;
            }

            UpdateBrainState(distance, ref brn, ntt, pos, target, targetPos);
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
                brn.State = BrainState.Approaching;
                break;
            }
        }

        private void UpdateBrainState(int distance, ref BrainComponent brn, in NTT ntt, in PositionComponent pos, in NTT target, in PositionComponent targetPos)
        {
            brn.State = distance > 1 ? BrainState.Approaching : BrainState.Attacking;

            if (brn.State == BrainState.Approaching)
            {
                var dir = CoMath.GetRawDirection(targetPos.Position, pos.Position);
                var wlk = new WalkComponent(dir, false);
                ntt.Set(ref wlk);
                if (IsLogging)
                    Logger.Debug("{Entity} walking {dir} to {target}", ntt, (Direction)dir, target);
            }
            else if (brn.State == BrainState.Attacking)
            {
                var atk = new AttackComponent(in target, MsgInteractType.Physical);
                ntt.Set(ref atk);
                if (IsLogging)
                    Logger.Debug("{Entity} attacking {target}", ntt, target);
            }

            brn.State = BrainState.Sleeping;
            brn.SleepTicks = (int)(NttWorld.TargetTps * (1 + Random.Shared.NextSingle()));
        }
    }
}
