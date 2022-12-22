using System.Numerics;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class BasicAISystem : PixelSystem<PositionComponent, ViewportComponent, BrainComponent>
    {
        public BasicAISystem() : base("Basic AI System", threads: 1) { }
        protected override bool MatchesFilter(in PixelEntity ntt) => ntt.Type == EntityType.Monster && base.MatchesFilter(in ntt);

        public override void Update(in PixelEntity ntt, ref PositionComponent pos, ref ViewportComponent vwp, ref BrainComponent brn)
        {
            if (brn.State == BrainState.Idle)
                return;
            
            if(brn.State == BrainState.Sleeping)
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

                    brn.TargetId = b.Id;
                    brn.State = BrainState.Approaching;
                    break;
                }
            }

            if (brn.TargetId == 0)
                return;

            ref readonly var target = ref PixelWorld.GetEntity(brn.TargetId);
            ref readonly var targetPos = ref target.Get<PositionComponent>();

            var distance = Vector2.Distance(pos.Position, targetPos.Position);
            if(distance > 16)
            {
                brn.TargetId = 0;
                brn.State = BrainState.Idle;
                return;
            }

            if (distance > 2)
                brn.State = BrainState.Approaching;
            else
                brn.State = BrainState.Attacking;

            if (brn.State == BrainState.Approaching)
            {
                var dir = CoMath.GetRawDirection(targetPos.Position,pos.Position);

                var wlk = new WalkComponent(ntt.Id, dir, true);
                ntt.Add(ref wlk);
            }

            brn.State = BrainState.Sleeping;
            brn.SleepTicks = PixelWorld.TargetTps * 2;
        }
    }
}
