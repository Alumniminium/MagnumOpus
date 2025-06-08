using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Helpers;
using NttECS.ECS;

namespace MagnumOpus.Systems
{
    public sealed class MagicAttackSystem : NttSystem<TargetCollectionComponent>
    {
        public MagicAttackSystem() : base("Magic Attack", threads: 1, log: false) { }

        // Executes magic attacks against collected targets, applying damage and sending visual effects
        // to clients. Processes spell effects after targeting systems have determined affected entities.
        // Broadcasts magic effect packets for animations and applies damage components to each target
        // while skipping already dead entities to prevent unnecessary processing.
        public override void Update(in NTT ntt, ref TargetCollectionComponent tcc)
        {
            // === BROADCAST MAGIC EFFECT ANIMATIONS ===
            // Send visual effects to nearby players for spell animations
            var effectPackets = MsgMagicEffect.Create(in ntt, tcc.Targets, (int)tcc.MagicType.Power, (ushort)tcc.MagicType.MagicType, (byte)tcc.MagicType.Level);
            foreach (var packet in effectPackets)
                ntt.NetSync(packet, broadcast: true);

            // === APPLY DAMAGE TO TARGETS ===
            for (var i = 0; i < tcc.Targets.Count; i++)
            {
                var targetEntity = tcc.Targets[i];
                // Skip already dead entities
                if (targetEntity.Has<DeathTagComponent>())
                    continue;

                // Create damage component for target
                var damageComponent = new DamageComponent(in targetEntity, in ntt, (int)tcc.MagicType.Power);
                targetEntity.Set(ref damageComponent);

                if (IsLogging)
                    FConsole.WriteLine("{caster} attacking {target} with {spell}:{level}", ntt, targetEntity, tcc.MagicType.MagicType, tcc.MagicType.Level);
            }

            // Clean up the target collection
            ntt.Remove<TargetCollectionComponent>();
        }
    }
}