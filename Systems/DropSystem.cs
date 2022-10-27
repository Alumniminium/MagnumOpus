using MagnumOpus.ECS;
using MagnumOpus.Components;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class DropSystem : PixelSystem<DeathTagComponent, BodyComponent, DropResourceComponent>
    {
        public DropSystem() : base("Drop System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref DeathTagComponent dtc, ref BodyComponent bdy, ref DropResourceComponent pik)
        {

        }
    }
}