using System.Numerics;
using MagnumOpus.Components;
using MagnumOpus.ECS;

namespace MagnumOpus.AOGP.Actions
{
    public class JumpApproachAction : GOAPAction
    {
        public override int Cost { get; set; } = 5;
        public override bool PreconditionsFulfilled(in NTT ntt)
        {
            ref readonly var brn = ref ntt.Get<BrainComponent>();

            if (brn.Target == 0)
                return false;

            ref readonly var pos = ref ntt.Get<PositionComponent>();
            var distance = Vector2.Distance(pos.Position, NttWorld.GetEntity(brn.Target).Get<PositionComponent>().Position);

            return distance > 12;
        }

        public override void Execute(in NTT ntt)
        {
            ref readonly var pos = ref ntt.Get<PositionComponent>();
            ref readonly var brn = ref ntt.Get<BrainComponent>();

            var targetPos = NttWorld.GetEntity(brn.Target).Get<PositionComponent>().Position;

            var jmp = new JumpComponent((ushort)targetPos.X, (ushort)targetPos.Y);
            ntt.Set(ref jmp);
        }
    }
}
