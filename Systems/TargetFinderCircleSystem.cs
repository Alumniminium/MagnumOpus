using System.Numerics;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;

namespace MagnumOpus.Systems
{
    public sealed class TargetFinderCircleSystem : NttSystem<CircleTargetComponent, ViewportComponent>
    {
        public TargetFinderCircleSystem() : base("Radius Targets", threads: 2) { }

        public override void Update(in NTT ntt, ref CircleTargetComponent atk, ref ViewportComponent vwp)
        {
            var tcc = new TargetCollectionComponent(atk.MagicType);

            foreach (var kvp in vwp.EntitiesVisible)
            {
                var b = kvp.Value;
                ref readonly var bPos = ref b.Get<PositionComponent>();
                var dir = CoMath.GetDirection(bPos.Position, new Vector2(atk.X, atk.Y));
                var range = atk.MagicType.Distance + 1;

                if (dir == Direction.South)
                    range += 2;
                if (dir is Direction.SouthWest or Direction.SouthEast)
                    range += 1;
                if (dir is Direction.West or Direction.East)
                    range -= 1;

                if (!CoMath.InRange(new Vector2(atk.X, atk.Y), bPos.Position, range))
                    continue;

                if (b.Type == EntityType.Player && atk.MagicType.Crime != 0)
                    continue; // TODO: Check if player is in PK mode

                if (b.Has<DeathTagComponent>())
                    continue;

                tcc.Targets.Add(b);
                if (IsLogging)
                    Logger.Debug("{ntt} adding {b} to target list of {skill}:{level}", ntt, b, atk.MagicType.MagicType, atk.MagicType.Level);
            }
            ntt.Set(ref tcc);
            ntt.Remove<CircleTargetComponent>();
        }
    }
}