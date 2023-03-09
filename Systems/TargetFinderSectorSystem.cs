using System.Numerics;
using MagnumOpus.Components;
using MagnumOpus.Components.Attack;
using MagnumOpus.Components.Death;
using MagnumOpus.Components.Entity;
using MagnumOpus.Components.Location;
using MagnumOpus.ECS;
using MagnumOpus.Helpers;

namespace MagnumOpus.Systems
{
    public sealed class TargetFinderSectorSystem : NttSystem<SectorTargetComponent, PositionComponent, ViewportComponent>
    {
        public TargetFinderSectorSystem() : base("Sector Targets", threads: 2) { }

        public override void Update(in NTT ntt, ref SectorTargetComponent atk, ref PositionComponent pos, ref ViewportComponent vwp)
        {
            var tcc = new TargetCollectionComponent( atk.MagicType);

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
            ntt.Set(ref tcc);
            ntt.Remove<SectorTargetComponent>();
        }
    }
}