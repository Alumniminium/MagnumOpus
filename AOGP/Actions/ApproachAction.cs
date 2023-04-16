using System.Numerics;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Helpers;

namespace MagnumOpus.AOGP.Actions
{
    public class WalkApproachAction : GOAPAction
    {
        public override int Cost { get; set; }
        public override bool PreconditionsFulfilled(in NTT ntt)
        {
            ref readonly var pos = ref ntt.Get<PositionComponent>();
            ref readonly var brn = ref ntt.Get<BrainComponent>();

            Cost = 2 + (int)Vector2.Distance(pos.Position, NttWorld.GetEntity(brn.Target).Get<PositionComponent>().Position);

            return brn.Target != 0 && Vector2.Distance(pos.Position, NttWorld.GetEntity(brn.Target).Get<PositionComponent>().Position) > 1;
        }

        public override void Execute(in NTT ntt)
        {
            ref readonly var pos = ref ntt.Get<PositionComponent>();
            ref readonly var brn = ref ntt.Get<BrainComponent>();

            var targetPos = NttWorld.GetEntity(brn.Target).Get<PositionComponent>().Position;
            var dir = CoMath.GetRawDirection(targetPos, pos.Position);
            var wlk = new WalkComponent(dir, false);
            ntt.Set(ref wlk);
        }
    }
}