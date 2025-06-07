using System.Collections.Concurrent;
using System.Numerics;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Helpers;
using NttECS.ECS;

namespace MagnumOpus.Systems
{
    /// <summary>
    /// Implements flocking behavior for entities using boid algorithms (alignment, cohesion, separation).
    /// Manages group movement patterns and target seeking for entities that exhibit swarm intelligence.
    /// </summary>
    public sealed class BoidSystem : NttSystem<LifeGiverComponent, BoidBehaviorComponent, PositionComponent, ViewportComponent>
    {
        private readonly ConcurrentDictionary<int, Vector2> _flockTargets = new();

        /// <summary>
        /// Initializes the BoidSystem with full multi-threaded processing capabilities.
        /// </summary>
        public BoidSystem() : base("Boid", threads: 1) { }

        /// <summary>
        /// Processes boid flocking behavior including alignment, cohesion, separation, and target seeking.
        /// </summary>
        /// <param name="ntt">The entity exhibiting boid behavior</param>
        /// <param name="lgc">Life giver component for spawner-based target generation</param>
        /// <param name="boi">Boid behavior component containing flock ID and target data</param>
        /// <param name="pos">Position component for movement calculations</param>
        /// <param name="vwp">Viewport component for detecting nearby flock members</param>
        public override void Update(in NTT ntt, ref LifeGiverComponent lgc, ref BoidBehaviorComponent boi, ref PositionComponent pos, ref ViewportComponent vwp)
        {
            if ((Tick - boi.UpdateOffset) % NttWorld.TargetTps * 0.5 != 0)
                return;

            // Boid behavior parameters
            var separationDistance = 1;

            var alignment = Vector2.Zero;
            var cohesion = Vector2.Zero;
            var separation = Vector2.Zero;

            var alignmentCount = 0;
            var cohesionCount = 0;
            var separationCount = 0;

            // Behavior weights
            var alignmentWeight = 3.0f;
            var cohesionWeight = 1.0f;
            var separationWeight = 1.0f;
            var targetSeekingWeight = 5.0f;

            // Calculate average alignment, cohesion, and separation
            foreach (var otherEntity in vwp.EntitiesVisible)
            {
                if (otherEntity == ntt)
                    continue;

                ref var otherBoid = ref otherEntity.Get<BoidBehaviorComponent>();

                if (otherBoid.Flock != boi.Flock)
                    continue;

                ref readonly var otherPosition = ref otherEntity.Get<PositionComponent>();
                var distance = Vector2.Distance(pos.Position, otherPosition.Position);

                // Alignment
                alignment += CoMath.DirectionToVector(otherPosition.Direction);
                alignmentCount++;

                // Cohesion
                cohesion += otherPosition.Position;
                cohesionCount++;

                // Separation
                if (distance < separationDistance)
                {
                    separation += pos.Position - otherPosition.Position;
                    separationCount++;
                }
            }

            if (alignmentCount > 0)
                alignment /= alignmentCount;

            if (cohesionCount > 0)
            {
                cohesion /= cohesionCount;
                cohesion = Vector2.Normalize(cohesion - pos.Position);
            }

            if (separationCount > 0)
                separation /= separationCount;
            if (!_flockTargets.ContainsKey(boi.Flock) || Tick % (NttWorld.TargetTps * 30) == 0)
            {
                ref readonly var spawnerPosition = ref lgc.NTT.Get<PositionComponent>();
                var angle = (float)(Random.Shared.NextDouble() * 2 * Math.PI);
                var radius = (float)(Random.Shared.NextDouble() * 50);
                _flockTargets[boi.Flock] = spawnerPosition.Position + new Vector2(radius * MathF.Cos(angle), radius * MathF.Sin(angle));
            }
            boi.Target = _flockTargets[boi.Flock];

            if (Vector2.Distance(pos.Position, boi.Target) <= 1)
                return;

            var targetSeeking = CoMath.DirectionToVector(CoMath.GetDirection(boi.Target, pos.Position));
            // Combine the boid behaviors with appropriate weights
            var newDirection = alignment * alignmentWeight + cohesion * cohesionWeight + separation * separationWeight + targetSeeking * targetSeekingWeight;

            if (newDirection != Vector2.Zero)
            {
                // Move the entity one grid cell at a time
                newDirection = Vector2.Normalize(newDirection);
                var nearestDirection = CoMath.GetNearestDirection(newDirection);
                pos.Direction = nearestDirection;
                // Create a new WalkComponent with the nearest direction
                var walkComponent = new WalkComponent((byte)nearestDirection, true);
                ntt.Set(ref walkComponent);
            }
        }
    }
}