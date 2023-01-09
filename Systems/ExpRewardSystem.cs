using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Squiggly;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class ExpRewardSystem : PixelSystem<LevelComponent, ExpRewardComponent>
    {
        public ExpRewardSystem() : base("Exp Reward System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref LevelComponent lvl, ref ExpRewardComponent rew)
        {
            lvl.Experience += (uint)rew.Experience;

            if(lvl.Experience >= lvl.ExperienceToNextLevel)
            {
                ref var hlt = ref ntt.Get<HealthComponent>();
                lvl.Level++;
                lvl.Experience = 0;
                lvl.ExperienceToNextLevel = (uint)Collections.LevelExps[lvl.Level].ExpReq;
                lvl.ChangedTick = PixelWorld.Tick;
                hlt.Health = hlt.MaxHealth;

                var lvlUp = MsgUserAttrib.Create(ntt.NetId, lvl.Level, Enums.MsgUserAttribType.Level);
                ntt.NetSync(ref lvlUp, false);

                var lvlActionMsg = MsgAction.Create(ntt.NetId,0,0,0,0, Enums.MsgActionType.LevelUp);
                ntt.NetSync(ref lvlActionMsg, true);
            }

            var expUp = MsgUserAttrib.Create(ntt.NetId, lvl.Experience, Enums.MsgUserAttribType.Experience);
            ntt.NetSync(ref expUp, false);
            ntt.Remove<ExpRewardComponent>();
        }
    }
}