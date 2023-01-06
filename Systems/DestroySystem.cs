using MagnumOpus.Components;
using MagnumOpus.ECS;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class DestroySystem : PixelSystem<DestroyEndOfFrameComponent>
    {
        public DestroySystem() : base("Destroy System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref DestroyEndOfFrameComponent def) => PixelWorld.Destroy(in ntt);
    }
}