using System.Numerics;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;

namespace MagnumOpus.AOGP.Actions
{
    public class AttackAction : GOAPAction
    {
        public override int Cost { get; set; } = 2;
        public override bool PreconditionsFulfilled(in NTT ntt)
        {
            ref readonly var pos = ref ntt.Get<PositionComponent>();
            ref readonly var brn = ref ntt.Get<BrainComponent>();

            // Check if there's a target and if the target is within attack range
            return brn.Target != 0 && Vector2.Distance(pos.Position, NttWorld.GetEntity(brn.Target).Get<PositionComponent>().Position) <= 1;
        }

        public override void Execute(in NTT ntt)
        {
            ref readonly var brn = ref ntt.Get<BrainComponent>();

            var atk = new AttackComponent(in NttWorld.GetEntity(brn.Target), MsgInteractType.Physical);
            ntt.Set(ref atk);
        }

        public override void UpdateEffects(in NTT ntt)
        {
            // No specific effects to update after execution
        }
    }
}