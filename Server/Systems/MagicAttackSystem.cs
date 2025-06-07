using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Helpers;
using NttECS.ECS;

namespace MagnumOpus.Systems
{
    /// <summary>
    /// Executes magic attacks against collected targets, applying damage and sending visual effects to clients.
    /// Processes spell effects after targeting systems have determined affected entities.
    /// </summary>
    public sealed class MagicAttackSystem : NttSystem<TargetCollectionComponent>
    {
        /// <summary>
        /// Initializes the MagicAttackSystem with limited threading for spell execution.
        /// </summary>
        public MagicAttackSystem() : base("Magic Attack", threads: 1) { }

        /// <summary>
        /// Executes magic attacks against all targets in the collection, applying damage and visual effects.
        /// </summary>
        /// <param name="ntt">The entity casting the spell</param>
        /// <param name="targetCollection">Target collection component containing affected entities and spell data</param>
        public override void Update(in NTT ntt, ref TargetCollectionComponent targetCollection)
        {
            var effectMessages = MsgMagicEffect.Create(in ntt, targetCollection.Targets, (int)targetCollection.MagicType.Power, (ushort)targetCollection.MagicType.MagicType, (byte)targetCollection.MagicType.Level);
            foreach (var effectMessage in effectMessages)
            {
                ntt.NetSync(effectMessage, true);
            }
            foreach (var targetEntity in targetCollection.Targets)
            {
                var damageComponent = new DamageComponent(in targetEntity, in ntt, (int)targetCollection.MagicType.Power);
                targetEntity.Set(ref damageComponent);

                if (IsLogging)
                    FConsole.WriteLine("{ntt} attacking {target} with {skill}:{level}", ntt, targetEntity, targetCollection.MagicType.MagicType, targetCollection.MagicType.Level);
            }
            ntt.Remove<TargetCollectionComponent>();
        }
    }
}