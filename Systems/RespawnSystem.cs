using MagnumOpus.ECS;
using MagnumOpus.Simulation.Components;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class RespawnSystem : PixelSystem<RespawnTagComponent, BodyComponent, LevelComponent, HealthComponent>
    {
        public RespawnSystem() : base("Respawn System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref RespawnTagComponent rtc, ref BodyComponent bdy, ref LevelComponent lvl, ref HealthComponent hlt)
        {
            if (rtc.RespawnTimeTick > ConquerWorld.Tick)
                return;

           
        }
    }
}