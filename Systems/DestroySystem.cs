using MagnumOpus.Components;
using MagnumOpus.ECS;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class DestroySystem : NttSystem<DestroyEndOfFrameComponent>
    {
        public DestroySystem() : base("Destroy", threads: 1) { }

        public override void Update(in NTT ntt, ref DestroyEndOfFrameComponent def) => NttWorld.Destroy(in ntt);
    }
}