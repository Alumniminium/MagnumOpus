using MagnumOpus.ECS;
using MagnumOpus.Networking;
using MagnumOpus.Components;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class DeathSystem : PixelSystem<DeathTagComponent>
    {
        public DeathSystem() : base("Death System", threads: 1) { }

        public override void Update(in PixelEntity ntt, ref DeathTagComponent dtc)
        {
            if (dtc.KillerId != 0)
            {
                if (!PixelWorld.EntityExists(dtc.KillerId))
                    return;
                var killer = PixelWorld.GetEntity(dtc.KillerId);
                if (killer.Has<NameTagComponent>() && ntt.Has<NameTagComponent>())
                {
                    ref readonly var killerNameTag = ref killer.Get<NameTagComponent>();
                    ref readonly var killedNameTag = ref ntt.Get<NameTagComponent>();
                    NetworkHelper.Broadcast(MsgText.Create(killerNameTag.Name, killedNameTag.Name, $"{killedNameTag.Name} was killed by {killerNameTag.Name}", MsgTextType.Service));
                }
            }
            if (ntt.Type == EntityType.Player)
            {
                var rtc = new RespawnTagComponent(ntt.Id, 1000, 5);
                ntt.Add(ref rtc);
                ntt.Remove<DeathTagComponent>();
                ntt.Remove<InputComponent>();
                return;
            }

            ref readonly var pos = ref ntt.Get<PositionComponent>();
            Game.Grids[pos.Map].Remove(in ntt);
            PixelWorld.Destroy(in ntt);
        }
    }
}