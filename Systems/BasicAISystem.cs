using System.Numerics;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Squiggly;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class BasicAISystem : NttSystem<PositionComponent, ViewportComponent, BrainComponent>
    {
        public BasicAISystem() : base("Basic AI", threads: 12) { }
        protected override bool MatchesFilter(in NTT ntt) => ntt.Type == EntityType.Monster && !ntt.Has<GuardPositionComponent>() && base.MatchesFilter(in ntt);

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
                vwp.EntitiesVisible.Clear();
                Collections.SpatialHashs[pos.Map].GetVisibleEntities(ref vwp);
                if (Trace) 
                    Logger.Debug("Waking up {ntt} with {visibleCount} visible entities", ntt, vwp.EntitiesVisible.Count);
            }

            if (brn.TargetId == 0)
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

                if (brn.TargetId == 0)
                {
                    brn.State = BrainState.Idle;
                    return;
                }
            }

            ref readonly var target = ref NttWorld.GetEntity(brn.TargetId);
            ref readonly var targetPos = ref target.Get<PositionComponent>();

            if (target.Has<DeathTagComponent>())
            {
                brn.TargetId = 0;
                return;
            }

            if (brn.TargetId == 0)
            {
                brn.State = BrainState.Idle;
                return;
            }


            var distance = (int)Vector2.Distance(pos.Position, targetPos.Position);
            if (distance > 16)
            {
                brn.TargetId = 0;
                brn.State = BrainState.Idle;
                if (Trace) 
                    Logger.Debug("{Entity} target {target} out of range", ntt, target);
                return;
            }

            if (distance > 1)
                brn.State = BrainState.Approaching;
            else
                brn.State = BrainState.Attacking;

            if (brn.State == BrainState.Approaching)
            {
                var dir = CoMath.GetRawDirection(targetPos.Position, pos.Position);

                var wlk = new WalkComponent(ntt.Id, dir, false);
                ntt.Set(ref wlk);
                if (Trace) 
                    Logger.Debug("{Entity} walking {dir} to {target}", ntt, (Direction)dir, target);
            }
            if (brn.State == BrainState.Attacking)
            {
                var atk = new AttackComponent(ntt.Id, in target, MsgInteractType.Physical);
                ntt.Set(ref atk);
                if (Trace) 
                    Logger.Debug("{Entity} attacking {target}", ntt, target);
            }

            brn.State = BrainState.Sleeping;
            brn.SleepTicks = (int)(NttWorld.TargetTps * (1+Random.Shared.NextSingle()));
        }
    }
}
