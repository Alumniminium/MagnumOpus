using System.Numerics;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class TargetFinderLineSystem : NttSystem<LineTargetComponent, PositionComponent, ViewportComponent>
    {
        public TargetFinderLineSystem() : base("Linear Targets", threads: Environment.ProcessorCount) { }

        public override void Update(in NTT ntt, ref LineTargetComponent atk, ref PositionComponent pos, ref ViewportComponent vwp)
        {
            var tcc = new TargetCollectionComponent(ntt.Id, atk.MagicType);


            foreach (var kvp in vwp.EntitiesVisible)
            {
                var b = kvp.Value;
                ref readonly var bPos = ref b.Get<PositionComponent>();

                if (b.Type == EntityType.Player && atk.MagicType.Crime != 0)
                    continue; // TODO: Check if player is in PK mode

                if (b.Has<DeathTagComponent>())
                    continue;

                if (!CoMath.DdaLine(pos.Position, new Vector2(atk.X, atk.Y), atk.MagicType.Distance, bPos.Position))
                    continue;
                
                tcc.Targets.Add(b);
                Logger.Debug("{ntt} adding {b} to target list of {skill}:{level}", ntt, b, atk.MagicType.MagicType, atk.MagicType.Level);
            }

            var msgEffect = MsgMagicEffect.Create(in ntt, atk.X, atk.Y, (ushort)atk.MagicType.MagicType, (byte)atk.MagicType.Level);
            ntt.NetSync(ref msgEffect);

            ntt.Set(ref tcc);
            ntt.Remove<LineTargetComponent>();
        }
    }
}