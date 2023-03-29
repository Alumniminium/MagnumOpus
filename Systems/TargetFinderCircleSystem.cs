using System.Numerics;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;

namespace MagnumOpus.Systems
{
    public sealed class TargetFinderSystem : NttSystem<TargetingComponent, ViewportComponent>
    {
        public TargetFinderSystem() : base("Radius Targets", threads: 2) { }

        public override void Update(in NTT ntt, ref TargetingComponent atk, ref ViewportComponent vwp)
        {
            var tcc = new TargetCollectionComponent(atk.MagicType);
            ref readonly var pos = ref ntt.Get<PositionComponent>();

            switch (atk.TargetingType)
            {
                case TargetingType.Circle:
                    {
                        foreach (var kvp in vwp.EntitiesVisible)
                        {
                            var b = kvp.Value;
                            ref readonly var bPos = ref b.Get<PositionComponent>();
                            var dir = CoMath.GetDirection(bPos.Position, new Vector2(atk.X, atk.Y));
                            var range = atk.MagicType.Distance + 1;

                            if (dir is Direction.South)
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
                        break;
                    }
                case TargetingType.Line:
                    {
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
                            if (IsLogging)
                                Logger.Debug("{ntt} adding {b} to target list of {skill}:{level}", ntt, b, atk.MagicType.MagicType, atk.MagicType.Level);
                        }
                        break;
                    }
                case TargetingType.Sector:
                    {
                        foreach (var kvp in vwp.EntitiesVisible)
                        {
                            var b = kvp.Value;
                            ref readonly var bPos = ref b.Get<PositionComponent>();

                            if (b.Type == EntityType.Player && atk.MagicType.Crime != 0)
                                continue; // TODO: Check if player is in PK mode

                            if (b.Has<DeathTagComponent>())
                                continue;

                            if (!CoMath.InSector(pos.Position, new Vector2(atk.X, atk.Y), bPos.Position, atk.MagicType.Range * 10 * MathF.PI / 180))
                                continue;

                            tcc.Targets.Add(b);
                            if (IsLogging)
                                Logger.Debug("{ntt} adding {b} to target list of {skill}:{level}", ntt, b, atk.MagicType.MagicType, atk.MagicType.Level);

                        }
                        break;
                    }
            }
            ntt.Set(ref tcc);
            ntt.Remove<TargetingComponent>();
        }
    }
}