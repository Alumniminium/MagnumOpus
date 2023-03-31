using System.Collections.Concurrent;
using System.Numerics;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Helpers;

namespace MagnumOpus.Systems
{
    public sealed class BoidSystem : NttSystem<LifeGiverComponent, BoidBehaviorComponent, PositionComponent, ViewportComponent>
    {
        private readonly ConcurrentDictionary<int, Vector2> _flockTargets = new();

        public BoidSystem() : base("Boid", threads: Environment.ProcessorCount) { }

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
            foreach (var kvp in vwp.EntitiesVisible)
            {
                var otherNtt = kvp.Value;

                if (otherNtt == ntt)
                    continue;

                ref var otherBoi = ref otherNtt.Get<BoidBehaviorComponent>();

                if (otherBoi.Flock != boi.Flock)
                    continue;

                ref readonly var otherPos = ref otherNtt.Get<PositionComponent>();
                var distance = Vector2.Distance(pos.Position, otherPos.Position);

                // Alignment
                alignment += CoMath.DirectionToVector(otherPos.Direction);
                alignmentCount++;

                // Cohesion
                cohesion += otherPos.Position;
                cohesionCount++;

                // Separation
                if (distance < separationDistance)
                {
                    separation += pos.Position - otherPos.Position;
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
                ref readonly var lgcPos = ref lgc.NTT.Get<PositionComponent>();
                var angle = (float)(Random.Shared.NextDouble() * 2 * Math.PI);
                var radius = (float)(Random.Shared.NextDouble() * 50);
                _flockTargets[boi.Flock] = lgcPos.Position + new Vector2(radius * MathF.Cos(angle), radius * MathF.Sin(angle));
                // Logger.Information($"New flock target for {boi.Flock}: {_flockTargets[boi.Flock]}");
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