using System.Numerics;
using NttECS.ECS;
namespace MagnumOpus.Components
{
    [Component(SaveEnabled: true)]
    public struct BoidBehaviorComponent(int flock, Vector2 target)
    {
        public int Flock = flock;
        public Vector2 Target = target;
        public int UpdateOffset = Random.Shared.Next(0, NttWorld.TargetTps + 1);
    }
}
