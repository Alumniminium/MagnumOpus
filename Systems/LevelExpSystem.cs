using MagnumOpus.ECS;
using MagnumOpus.Simulation.Components;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class LevelExpSystem : PixelSystem<LevelComponent, ExpRewardComponent>
    {
        public LevelExpSystem() : base("Level & Exp System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref LevelComponent lvl, ref ExpRewardComponent exp)
        {
            lvl.Experience += exp.Experience;
            lvl.ChangedTick = PixelWorld.Tick;
            // LeaderBoard.Add(new LeaderBoard.LeaderBoardEntry { Name = ntt.Get<NameTagComponent>().Name, Score = lvl.Experience });
            ntt.Remove<ExpRewardComponent>();

            if (lvl.Experience < lvl.ExperienceToNextLevel)
                return;

            lvl.Level++;
            lvl.Experience = 0;
            lvl.ExperienceToNextLevel = (uint)(lvl.ExperienceToNextLevel * 1.25f);

            ref var bdy = ref ntt.Get<BodyComponent>();
            ref var vwp = ref ntt.Get<ViewportComponent>();

            vwp.Viewport.Width *= 1.03f;
            vwp.Viewport.Height *= 1.03f;
            bdy.ChangedTick = PixelWorld.Tick;
        }
    }
}
