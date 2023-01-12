using System.Numerics;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Helpers;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class LineTargetFinderSystem : PixelSystem<LineTargetComponent, PositionComponent, ViewportComponent>
    {
        public LineTargetFinderSystem() : base("Line Target Finder System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref LineTargetComponent atk, ref PositionComponent pos, ref ViewportComponent vwp)
        {
            var tcc = new TargetCollectionComponent(ntt.Id, atk.MagicType);

            for (int i = 0; i < vwp.EntitiesVisible.Count; i++)
            {
                var b = vwp.EntitiesVisible[i];
                ref readonly var bPos = ref b.Get<PositionComponent>();

                if (b.Type == EntityType.Player && atk.MagicType.Crime != 0)
                    continue; // TODO: Check if player is in PK mode

                if (CoMath.DdaLine(pos.Position, new Vector2(atk.X, atk.Y), atk.MagicType.Distance, bPos.Position))
                    tcc.Targets.Add(b);
            }
            ntt.Set(ref tcc);
            ntt.Remove<LineTargetComponent>();
        }
    }
}