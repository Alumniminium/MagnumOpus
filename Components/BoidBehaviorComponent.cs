using System.Numerics;
using MagnumOpus.ECS;
namespace MagnumOpus.Components
{
    [Component(saveEnabled: true)]
    public struct BoidBehaviorComponent
    {
        public int Flock;
        public Vector2 Target;
        public int UpdateOffset;

        public BoidBehaviorComponent(int flock, Vector2 target)
        {
            Flock = flock;
            Target = target;
            UpdateOffset = Random.Shared.Next(0, NttWorld.TargetTps + 1);
        }
    }
}
