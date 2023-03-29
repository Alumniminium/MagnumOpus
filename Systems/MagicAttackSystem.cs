using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Systems
{
    public sealed class MagicAttackSystem : NttSystem<TargetCollectionComponent>
    {
        public MagicAttackSystem() : base("Magic Attack", threads: 2) { }

        public override void Update(in NTT ntt, ref TargetCollectionComponent atk)
        {
            foreach (var target in atk.Targets)
            {
                var msg = MsgMagicEffect.Create(in ntt, in target, (int)atk.MagicType.Power, (ushort)atk.MagicType.MagicType, (byte)atk.MagicType.Level);
                ntt.NetSync(ref msg, true);

                var dmg = new DamageComponent(in target, in ntt, (int)atk.MagicType.Power);
                target.Set(ref dmg);

                if (IsLogging)
                    Logger.Debug("{ntt} attacking {target} with {skill}:{level}", ntt, target, atk.MagicType.MagicType, atk.MagicType.Level);
            }
            ntt.Remove<TargetCollectionComponent>();
        }
    }
}