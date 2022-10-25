using HerstLib.Math;
using MagnumOpus.ECS;
using MagnumOpus.Simulation.Components;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class RespawnSystem : PixelSystem<RespawnTagComponent, BodyComponent, LevelComponent, HealthComponent>
    {
        public RespawnSystem() : base("Respawn System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref RespawnTagComponent rtc, ref BodyComponent phy, ref LevelComponent lvl, ref HealthComponent hlt)
        {
            if (rtc.RespawnTimeTick > Game.CurrentTick)
                return;

            lvl.Experience -= rtc.ExpPenalty;
            while (lvl.Experience < 0)
            {
                if (lvl.Level == 1)
                {
                    lvl.Experience = 0;
                    lvl.ExperienceToNextLevel = 100;
                    break;
                }
                lvl.Level--;
                lvl.Experience += lvl.Experience;
                lvl.ExperienceToNextLevel = (uint)(100f * lvl.Level * 1.25f);
            }
            
            hlt.Health = hlt.MaxHealth;
            // var spawn = SpawnManager.PlayerSpawnPoint;
            // phy.Location = spawn;
            
            phy.ChangedTick = Game.CurrentTick;
            ntt.Remove<RespawnTagComponent>();
            var inp = new InputComponent(ntt.Id, default, default, default);
            ntt.Add(ref inp);
        }
    }
}