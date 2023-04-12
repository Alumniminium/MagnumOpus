using System.Numerics;
using Co2Core.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;

namespace MagnumOpus.Systems
{
    public sealed class TargetFinderSystem : NttSystem<PositionComponent, TargetingComponent, ViewportComponent>
    {
        public TargetFinderSystem() : base("Radius Targets", threads: 2) { }

        public override void Update(in NTT ntt, ref PositionComponent pos, ref TargetingComponent atk, ref ViewportComponent vwp)
        {
            var tcc = new TargetCollectionComponent(atk.MagicType);

            foreach (var b in vwp.EntitiesVisible)
            {
                ref readonly var bPos = ref b.Get<PositionComponent>();

                if (!IsValidTarget(b, atk.MagicType))
                    continue;

                var addTarget = false;
                var atkPos = new Vector2(atk.X, atk.Y);

                switch (atk.TargetingType)
                {
                    case TargetingType.Circle:
                        var dir = CoMath.GetDirection(bPos.Position, atkPos);
                        var range = FixCircleRange(dir, atk.MagicType.Distance + 1);
                        addTarget = CoMath.InRange(atkPos, bPos.Position, range);
                        break;
                    case TargetingType.Line:
                        addTarget = CoMath.DdaLine(pos.Position, atkPos, atk.MagicType.Distance, bPos.Position);
                        break;
                    case TargetingType.Sector:
                        addTarget = CoMath.InSector(pos.Position, atkPos, bPos.Position, atk.MagicType.Range * 10 * MathF.PI / 180);
                        break;
                }

                if (addTarget)
                {
                    tcc.Targets.Add(b);
                    if (IsLogging)
                        Logger.Debug("{ntt} adding {b} to target list of {skill}:{level}", ntt, b, atk.MagicType.MagicType, atk.MagicType.Level);
                }
            }

            ntt.Set(ref tcc);
            ntt.Remove<TargetingComponent>();
        }

        private static uint FixCircleRange(Direction dir, uint range)
        {
            if (dir is Direction.South)
                range += 2;
            else if (dir is Direction.SouthWest or Direction.SouthEast)
                range += 1;
            else if (dir is Direction.West or Direction.East)
                range -= 1;
            return range;
        }

        private static bool IsValidTarget(in NTT target, in MagicType.Entry magicType)
        {
            if (target.Type == EntityType.Player && magicType.Crime != 0)
                return false; // TODO: Check if player is in PK mode

            if (target.Has<DeathTagComponent>())
                return false;

            return true;
        }
    }
}