using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class MagicAttackSystem : PixelSystem<TargetCollectionComponent>
    {
        public MagicAttackSystem() : base("Magic Attack System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref TargetCollectionComponent atk)
        {
            foreach(var target in atk.Targets)
            {
                var msg = MsgMagicEffect.Create(in ntt, in target, (int)atk.MagicType.Power, (ushort)atk.MagicType.MagicType, 0);
                ntt.NetSync(ref msg, true);

                var dmg = new DamageComponent(in target, in ntt, (int)atk.MagicType.Power);
                target.Set(ref dmg);
            }
            ntt.Remove<TargetCollectionComponent>();
        }
    }
}