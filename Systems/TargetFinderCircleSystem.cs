using System.Numerics;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class TargetFinderCircleSystem : NttSystem<CircleTargetComponent, ViewportComponent>
    {
        public TargetFinderCircleSystem() : base("O Targets", threads: Environment.ProcessorCount) { }

        public override void Update(in NTT ntt, ref CircleTargetComponent atk, ref ViewportComponent vwp)
        {
            var tcc = new TargetCollectionComponent(ntt.Id, atk.MagicType);
            for (int i = 0; i < vwp.EntitiesVisible.Count; i++)
            {
                var b = vwp.EntitiesVisible[i];
                ref readonly var bPos = ref b.Get<PositionComponent>();
                var dir = CoMath.GetDirection(bPos.Position, new Vector2(atk.X, atk.Y));
                var range = atk.MagicType.Distance+1;

                if(dir == Direction.South)
                    range += 2;
                if(dir == Direction.SouthWest || dir == Direction.SouthEast)
                    range += 1;
                if(dir == Direction.West || dir == Direction.East)
                    range -= 1;
                
                if (!CoMath.InRange(new Vector2(atk.X, atk.Y), bPos.Position, range))
                    continue;

                if (b.Type == EntityType.Player && atk.MagicType.Crime != 0)
                    continue; // TODO: Check if player is in PK mode

                if (b.Has<DeathTagComponent>())
                    continue;

                tcc.Targets.Add(b);
            }
            ntt.Set(ref tcc);
            ntt.Remove<CircleTargetComponent>();
        }
    }
}