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
            var msgs = MsgMagicEffect.Create(in ntt, atk.Targets, (int)atk.MagicType.Power, (ushort)atk.MagicType.MagicType, (byte)atk.MagicType.Level);
            foreach (var msg in msgs)
            {
                ntt.NetSync(msg, true);
            }
            foreach (var target in atk.Targets)
            {
                var dmg = new DamageComponent(in target, in ntt, (int)atk.MagicType.Power);
                target.Set(ref dmg);

                if (IsLogging)
                    Logger.Debug("{ntt} attacking {target} with {skill}:{level}", ntt, target, atk.MagicType.MagicType, atk.MagicType.Level);
            }
            ntt.Remove<TargetCollectionComponent>();
        }
    }
}