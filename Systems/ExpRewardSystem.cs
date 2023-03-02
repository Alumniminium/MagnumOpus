using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Squiggly;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class ExpRewardSystem : NttSystem<LevelComponent, ExpRewardComponent>
    {
        public ExpRewardSystem() : base("Exp Reward", threads: 2) { }

        public override void Update(in NTT ntt, ref LevelComponent lvl, ref ExpRewardComponent rew)
        {
            lvl.Experience += (uint)rew.Experience;

            if (lvl.Experience >= lvl.ExperienceToNextLevel)
            {
                ref var hlt = ref ntt.Get<HealthComponent>();
                lvl.Level++;
                var level = lvl.Level;
                var profession = ntt.Get<ProfessionComponent>().Profession;
                lvl.Experience = 0;
                lvl.ExperienceToNextLevel = (uint)Collections.LevelExps.Values[lvl.Level-1];
                lvl.ChangedTick = NttWorld.Tick;
                hlt.Health = hlt.MaxHealth;

                var allot = Collections.CqPointAllot.FirstOrDefault(x=> x.level == level && x.profession == ((long)profession / 10));
                if (allot != null)
                {
                    ref var pnt = ref ntt.Get<AttributeComponent>();
                    pnt.Agility = (ushort)allot.Speed;
                    pnt.Strength = (ushort)allot.force;
                    pnt.Vitality = (ushort)allot.health;
                    pnt.Spirit = (ushort)allot.soul;
                }

                var lvlActionMsg = MsgAction.Create(ntt.NetId, 0, 0, 0, 0, Enums.MsgActionType.LevelUp);
                ntt.NetSync(ref lvlActionMsg, true);
            }

            ntt.Remove<ExpRewardComponent>();
        }
    }
}