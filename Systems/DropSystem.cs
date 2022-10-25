using MagnumOpus.ECS;
using MagnumOpus.Simulation.Components;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class DropSystem : PixelSystem<DeathTagComponent, BodyComponent, DropResourceComponent>
    {
        public DropSystem() : base("Drop System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref DeathTagComponent dtc, ref BodyComponent phy, ref DropResourceComponent pik)
        {

        }
    }
}