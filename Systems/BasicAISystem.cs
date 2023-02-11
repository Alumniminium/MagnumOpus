using System.Numerics;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class BasicAISystem : NttSystem<PositionComponent, ViewportComponent, BrainComponent>
    {
        public BasicAISystem() : base("Basic AI") { }
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
                Game.Grids[pos.Map].GetVisibleEntities(ref vwp);
            }

            if (brn.TargetId == 0)
            {
                for (var i = 0; i < vwp.EntitiesVisible.Count; i++)
                {
                    var b = vwp.EntitiesVisible[i];

                    if (b.Type != EntityType.Player)
                        continue;

                    ref readonly var bEff = ref b.Get<StatusEffectComponent>();

                    if (bEff.Effects.HasFlag(StatusEffect.Dead))
                        brn.TargetId = 0;

                    brn.TargetId = b.Id;
                    brn.State = BrainState.Approaching;
                    break;
                }
            }


            if (brn.TargetId == 0)
            {
                brn.State = BrainState.Idle;
                return;
            }

            ref readonly var target = ref NttWorld.GetEntity(brn.TargetId);
            ref readonly var targetPos = ref target.Get<PositionComponent>();
            ref readonly var targetEff = ref target.Get<StatusEffectComponent>();

            if (targetEff.Effects.HasFlag(StatusEffect.Dead))
                brn.TargetId = 0;

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
            }
            if(brn.State == BrainState.Attacking)
            {
                var atk = new AttackComponent(ntt.Id, in target, MsgInteractType.Physical);
                ntt.Set(ref atk);
            }

            brn.State = BrainState.Sleeping;
            brn.SleepTicks = (int)(NttWorld.TargetTps * 2.5f);
        }
    }
}
