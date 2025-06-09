using System.Numerics;
using Co2Core.IO;
using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using NttECS.ECS;

namespace MagnumOpus.Systems;

public sealed class TargetFinderSystem : NttSystem<PositionComponent, TargetingComponent, ViewportComponent>
{
    public TargetFinderSystem() : base("Radius Targets", threads: 1, log: false) { }

    // Finds and collects targets for area-of-effect spells using geometric shapes like circles,
    // lines, and sectors. Validates targets based on spell crime flags and entity status before
    // adding to target collections. Uses mathematical algorithms for precise shape calculations
    // and includes direction-based range adjustments for visual perspective accuracy.
    public override void Update(in NTT ntt, ref PositionComponent pos, ref TargetingComponent tcc, ref ViewportComponent viewport)
    {
        var targetCollection = new TargetCollectionComponent(tcc.MagicType);
        var targetPos = new Vector2(tcc.X, tcc.Y);

        // === SCAN VISIBLE ENTITIES FOR TARGETS ===
        foreach (var visibleEntity in viewport.EntitiesVisible)
        {
            ref readonly var visibleNttPos = ref visibleEntity.Get<PositionComponent>();

            // Skip invalid targets (dead entities, crime restrictions, etc.)
            if (!IsValidTarget(visibleEntity, tcc.MagicType))
                continue;

            // === CALCULATE TARGET INCLUSION BASED ON SHAPE ===
            var shouldAddTarget = tcc.TargetingType switch
            {
                TargetingType.Circle => CoMath.InRange(targetPos, visibleNttPos.Position, FixCircleRange(CoMath.GetDirection(visibleNttPos.Position, targetPos), tcc.MagicType.Distance + 1)),
                TargetingType.Line => CoMath.DdaLine(pos.Position, targetPos, tcc.MagicType.Distance, visibleNttPos.Position),
                TargetingType.Sector => CoMath.InSector(pos.Position, targetPos, visibleNttPos.Position, tcc.MagicType.Range * 10 * MathF.PI / 180),
                _ => false
            };

            if (shouldAddTarget)
            {
                targetCollection.Targets.Add(visibleEntity);
                if (IsLogging)
                    FConsole.WriteLine("{caster} adding {target} to target list of {spell}:{level}", ntt, visibleEntity, tcc.MagicType.MagicType, tcc.MagicType.Level);
            }
        }

        // === FINALIZE TARGET COLLECTION ===
        ntt.Set(ref targetCollection);
        ntt.Remove<TargetingComponent>();
    }

    // Adjusts circle range based on direction to account for visual perspective differences
    private static uint FixCircleRange(Direction direction, uint range)
    {
        if (direction is Direction.South)
            range += 2;
        else if (direction is Direction.SouthWest or Direction.SouthEast)
            range += 1;
        else if (direction is Direction.West or Direction.East)
            range -= 1;
        return range;
    }

    // Validates whether an entity can be targeted by a spell based on crime flags and entity status
    private static bool IsValidTarget(in NTT targetEntity, in MagicType.Entry magicTypeEntry)
    {
        // Skip players when spell has crime flag (unless in PK mode)
        if (targetEntity.IsPlayer() && magicTypeEntry.Crime != 0)
            return false; // TODO: Check if player is in PK mode

        // Skip dead entities
        if (targetEntity.Has<DeathTagComponent>())
            return false;

        return true;
    }
}