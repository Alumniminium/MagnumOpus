using MagnumOpus.ECS;
using MagnumOpus.Simulation.Components;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class LevelExpSystem : PixelSystem<LevelComponent, ExperienceComponent, ExpRewardComponent>
    {
        public LevelExpSystem() : base("Level & Exp System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref LevelComponent lvl, ref ExperienceComponent exp, ref ExpRewardComponent rew)
        {

        }
    }
}
