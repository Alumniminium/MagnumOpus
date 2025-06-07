using System.Numerics;
using Co2Core.IO;
using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;

namespace MagnumOpus.Systems
{
    /// <summary>
    /// Finds and collects targets for area-of-effect spells using geometric shapes like circles, lines, and sectors.
    /// Validates targets based on spell crime flags and entity status before adding to target collections.
    /// </summary>
    public sealed class TargetFinderSystem : NttSystem<PositionComponent, TargetingComponent, ViewportComponent>
    {
        /// <summary>
        /// Initializes the TargetFinderSystem with limited threading for targeting calculations.
        /// </summary>
        public TargetFinderSystem() : base("Radius Targets", threads: 2) { }

        /// <summary>
        /// Processes targeting requests to find entities within spell area-of-effect shapes.
        /// </summary>
        /// <param name="ntt">The entity casting the spell</param>
        /// <param name="position">Position component for spell origin</param>
        /// <param name="targeting">Targeting component containing area-of-effect parameters</param>
        /// <param name="viewport">Viewport component with visible entities to check</param>
        public override void Update(in NTT ntt, ref PositionComponent position, ref TargetingComponent targeting, ref ViewportComponent viewport)
        {
            var targetCollection = new TargetCollectionComponent(targeting.MagicType);

            foreach (var visibleEntity in viewport.EntitiesVisible)
            {
                ref readonly var entityPosition = ref visibleEntity.Get<PositionComponent>();

                if (!IsValidTarget(visibleEntity, targeting.MagicType))
                    continue;

                var shouldAddTarget = false;
                var targetPosition = new Vector2(targeting.X, targeting.Y);

                switch (targeting.TargetingType)
                {
                    case TargetingType.Circle:
                        var direction = CoMath.GetDirection(entityPosition.Position, targetPosition);
                        var range = FixCircleRange(direction, targeting.MagicType.Distance + 1);
                        shouldAddTarget = CoMath.InRange(targetPosition, entityPosition.Position, range);
                        break;
                    case TargetingType.Line:
                        shouldAddTarget = CoMath.DdaLine(position.Position, targetPosition, targeting.MagicType.Distance, entityPosition.Position);
                        break;
                    case TargetingType.Sector:
                        shouldAddTarget = CoMath.InSector(position.Position, targetPosition, entityPosition.Position, targeting.MagicType.Range * 10 * MathF.PI / 180);
                        break;
                }

                if (shouldAddTarget)
                {
                    targetCollection.Targets.Add(visibleEntity);
                    if (IsLogging)
                        FConsole.WriteLine("{ntt} adding {b} to target list of {skill}:{level}", ntt, visibleEntity, targeting.MagicType.MagicType, targeting.MagicType.Level);
                }
            }

            ntt.Set(ref targetCollection);
            ntt.Remove<TargetingComponent>();
        }

        /// <summary>
        /// Adjusts circle range based on direction to account for visual perspective differences.
        /// </summary>
        /// <param name="direction">Direction from target to spell center</param>
        /// <param name="range">Base range to adjust</param>
        /// <returns>Adjusted range for more accurate circle targeting</returns>
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

        /// <summary>
        /// Validates whether an entity can be targeted by a spell based on crime flags and entity status.
        /// </summary>
        /// <param name="targetEntity">Entity to validate for targeting</param>
        /// <param name="magicTypeEntry">Magic type data containing crime and targeting rules</param>
        /// <returns>True if entity is a valid target for the spell</returns>
        private static bool IsValidTarget(in NTT targetEntity, in MagicType.Entry magicTypeEntry)
        {
            if (targetEntity.Type == EntityType.Player && magicTypeEntry.Crime != 0)
                return false; // TODO: Check if player is in PK mode

            if (targetEntity.Has<DeathTagComponent>())
                return false;

            return true;
        }
    }
}