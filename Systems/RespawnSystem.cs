using MagnumOpus.Components;
using MagnumOpus.ECS;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class RespawnSystem : NttSystem<RespawnTagComponent, BodyComponent, LevelComponent, HealthComponent>
    {
        public RespawnSystem() : base("Respawn", threads: Environment.ProcessorCount) { }

        public override void Update(in NTT ntt, ref RespawnTagComponent rtc, ref BodyComponent bdy, ref LevelComponent lvl, ref HealthComponent hlt)
        {
            if (rtc.RespawnTimeTick > NttWorld.Tick)
                return;

            
        }
    }
}