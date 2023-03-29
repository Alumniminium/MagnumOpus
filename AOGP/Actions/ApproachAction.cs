using System.Numerics;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Helpers;

namespace MagnumOpus.AOGP.Actions
{
    public class ApproachAction : GOAPAction
    {
        public override int Cost => 1;
        public override bool PreconditionsFulfilled(in NTT ntt)
        {
            ref readonly var pos = ref ntt.Get<PositionComponent>();
            ref readonly var brn = ref ntt.Get<BrainComponent>();

            return brn.TargetId != 0 && Vector2.Distance(pos.Position, NttWorld.GetEntity(brn.TargetId).Get<PositionComponent>().Position) > 1;
        }

        public override void Execute(in NTT ntt)
        {
            ref readonly var pos = ref ntt.Get<PositionComponent>();
            ref readonly var brn = ref ntt.Get<BrainComponent>();

            var targetPos = NttWorld.GetEntity(brn.TargetId).Get<PositionComponent>().Position;
            var dir = CoMath.GetRawDirection(targetPos, pos.Position);
            var wlk = new WalkComponent(dir, false);
            ntt.Set(ref wlk);
        }

        public override void UpdateEffects(in NTT ntt)
        {
            // No specific effects to update after execution
        }
    }
}