using System.Numerics;
using NttECS.ECS;
namespace MagnumOpus.Components
{
    [Component(SaveEnabled: true)]
    /// <summary>
    /// Flocking behavior component that enables entities to move in coordinated groups using 
    /// boid algorithms (alignment, cohesion, separation). Each entity belongs to a numbered 
    /// flock and moves toward dynamically generated targets near their spawner. UpdateOffset 
    /// distributes processing load across frames. Used by BoidSystem for group AI movement.
    /// </summary>
    public struct BoidBehaviorComponent(int flock, Vector2 target)
    {
        public int Flock = flock;
        public Vector2 Target = target;
        public int UpdateOffset = Random.Shared.Next(0, NttWorld.TargetTps + 1);
    }
}
