using System.Collections.Concurrent;
using System.Numerics;
using MagnumOpus.Components;
using MagnumOpus.Helpers;
using NttECS.ECS;

namespace MagnumOpus.Systems
{
    public sealed class BoidSystem : NttSystem<LifeGiverComponent, BoidBehaviorComponent, PositionComponent, ViewportComponent>
    {
        private readonly ConcurrentDictionary<int, Vector2> _flockTargets = new();

        public BoidSystem() : base("Boid", threads: 1, log: false) { }

        // Implements flocking behavior using classic boid algorithms. The system calculates three forces:
        // alignment (matching direction of nearby flock members), cohesion (moving toward the center of
        // the flock), and separation (avoiding crowding). It also adds target-seeking behavior where each
        // flock moves toward a randomly generated point near their spawner, refreshed every 30 seconds.
        public override void Update(in NTT ntt, ref LifeGiverComponent lgc, ref BoidBehaviorComponent boi, ref PositionComponent pos, ref ViewportComponent vwp)
        {
            // Only update twice per second, with offset for load distribution
            if ((Tick - boi.UpdateOffset) % NttWorld.TargetTps * 0.5 != 0)
                return;

            // === BOID BEHAVIOR CONFIGURATION ===
            const float SEPARATION_DISTANCE = 1f;
            const float ALIGNMENT_WEIGHT = 3.0f;
            const float COHESION_WEIGHT = 1.0f;
            const float SEPARATION_WEIGHT = 1.0f;
            const float TARGET_SEEKING_WEIGHT = 5.0f;

            // Initialize force accumulators
            var alignment = Vector2.Zero;
            var cohesion = Vector2.Zero;
            var separation = Vector2.Zero;
            var neighborCount = 0;

            // === CALCULATE BOID FORCES FROM FLOCK MEMBERS ===
            foreach (var otherEntity in vwp.EntitiesVisible)
            {
                // Skip self
                if (otherEntity == ntt)
                    continue;

                // Only interact with same flock members
                ref var otherBoid = ref otherEntity.Get<BoidBehaviorComponent>();
                if (otherBoid.Flock != boi.Flock)
                    continue;

                ref readonly var otherPosition = ref otherEntity.Get<PositionComponent>();
                var distance = Vector2.Distance(pos.Position, otherPosition.Position);

                // Alignment: match flock direction
                alignment += CoMath.DirectionToVector(otherPosition.Direction);

                // Cohesion: move toward flock center
                cohesion += otherPosition.Position;

                // Separation: avoid crowding neighbors
                if (distance < SEPARATION_DISTANCE)
                    separation += pos.Position - otherPosition.Position;

                neighborCount++;
            }

            // Average the accumulated forces
            if (neighborCount > 0)
            {
                alignment /= neighborCount;
                cohesion = Vector2.Normalize((cohesion / neighborCount) - pos.Position);
                if (separation != Vector2.Zero)
                    separation = Vector2.Normalize(separation);
            }

            // === UPDATE FLOCK TARGET POSITION ===
            // Generate new target every 30 seconds or if missing
            var needNewTarget = !_flockTargets.ContainsKey(boi.Flock) || Tick % (NttWorld.TargetTps * 30) == 0;
            if (needNewTarget)
            {
                ref readonly var spawnerPosition = ref lgc.NTT.Get<PositionComponent>();
                var angle = (float)(Random.Shared.NextDouble() * 2 * Math.PI);
                var radius = (float)(Random.Shared.NextDouble() * 50);
                var offset = new Vector2(radius * MathF.Cos(angle), radius * MathF.Sin(angle));
                _flockTargets[boi.Flock] = spawnerPosition.Position + offset;
            }
            boi.Target = _flockTargets[boi.Flock];

            // Skip movement if already at target
            if (Vector2.Distance(pos.Position, boi.Target) <= 1)
                return;

            // === COMBINE FORCES AND APPLY MOVEMENT ===
            var targetSeeking = CoMath.DirectionToVector(CoMath.GetDirection(boi.Target, pos.Position));

            var combinedForce = alignment * ALIGNMENT_WEIGHT +
                               cohesion * COHESION_WEIGHT +
                               separation * SEPARATION_WEIGHT +
                               targetSeeking * TARGET_SEEKING_WEIGHT;

            if (combinedForce != Vector2.Zero)
            {
                // Convert combined force to discrete movement direction
                combinedForce = Vector2.Normalize(combinedForce);
                var movementDirection = CoMath.GetNearestDirection(combinedForce);
                pos.Direction = movementDirection;

                // Apply movement via walk component
                var walkComponent = new WalkComponent((byte)movementDirection, true);
                ntt.Set(ref walkComponent);
            }
        }
    }
}