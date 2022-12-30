using MagnumOpus.ECS;
using MagnumOpus.Components;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class ExpRewardSystem : PixelSystem<ExperienceComponent, ExpRewardComponent>
    {
        public ExpRewardSystem() : base("Exp Reward System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref ExperienceComponent exp, ref ExpRewardComponent rew)
        {
            exp.Experience += rew.Experience;
            ntt.Remove<ExpRewardComponent>();

            if(exp.Experience >= exp.ExperienceToNextLevel)
            {
                ref var lvl = ref ntt.Get<LevelComponent>();
                ref var hlt = ref ntt.Get<HealthComponent>();
                lvl.Level++;
                lvl.ChangedTick = PixelWorld.Tick;
                hlt.Health = hlt.MaxHealth;

                var lvlUp = MsgUserAttrib.Create(ntt.NetId, lvl.Level, Enums.MsgUserAttribType.Level);
                ntt.NetSync(ref lvlUp, false);
            }

            var expUp = MsgUserAttrib.Create(ntt.NetId, exp.Experience, Enums.MsgUserAttribType.Experience);
            ntt.NetSync(ref expUp, false);
        }
    }
}