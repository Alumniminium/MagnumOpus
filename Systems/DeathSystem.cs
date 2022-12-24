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
            var death = MsgInteract.Create(in dtc.Killer, in ntt, MsgInteractType.Death, 0);
            var despwan = MsgAction.RemoveEntity(ntt.NetId);
            ntt.NetSync(ref despwan, true);
            ntt.NetSync(ref death, true);

            dtc.Killer.Remove<AttackComponent>();
            
            ref readonly var pos = ref ntt.Get<PositionComponent>();
            Game.Grids[pos.Map].Remove(in ntt);
            PixelWorld.Destroy(in ntt);
        }
    }
}