using System.Numerics;
using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct BoidBehaviorComponent(int flock, Vector2 target)
    {
        public int Flock = flock;
        public Vector2 Target = target;
        public int UpdateOffset = Random.Shared.Next(0, NttWorld.TargetTps + 1);
    }
}
