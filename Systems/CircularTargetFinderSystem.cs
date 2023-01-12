using System.Numerics;
using MagnumOpus.Components;
using MagnumOpus.ECS;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class CircularTargetFinderSystem : PixelSystem<CircleTargetComponent, ViewportComponent>
    {
        public CircularTargetFinderSystem() : base("Circular Target Finder System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref CircleTargetComponent atk, ref ViewportComponent vwp)
        {
            var tcc = new TargetCollectionComponent(ntt.Id, atk.MagicType);
            for (int i = 0; i < vwp.EntitiesVisible.Count; i++)
            {
                var b = vwp.EntitiesVisible[i];
                ref readonly var bPos = ref b.Get<PositionComponent>();

                var distance = Vector2.Distance(new Vector2(atk.X, atk.Y), bPos.Position);
                if (distance > atk.MagicType.Distance)
                    continue;

                if (b.Type == EntityType.Player && atk.MagicType.Crime != 0)
                    continue; // TODO: Check if player is in PK mode

                tcc.Targets.Add(b);
            }
            ntt.Set(ref tcc);
            ntt.Remove<CircleTargetComponent>();
        }
    }
}