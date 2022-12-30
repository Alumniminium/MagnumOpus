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
            // var despwan = MsgAction.RemoveEntity(ntt.NetId);
            // ntt.NetSync(ref despwan, true);
            ntt.NetSync(ref death, true);

            ref var eff = ref ntt.Get<StatusEffectComponent>();
            eff.Effects |= StatusEffect.Dead;

            if(ntt.Type == EntityType.Monster)
                eff.Effects |= StatusEffect.Fade;

            var update = MsgUserAttrib.Create(ntt.NetId, (ulong)eff.Effects, MsgUserAttribType.StatusEffect);
            ntt.NetSync(ref update, true);

            dtc.Killer.Remove<AttackComponent>();
            
            ref readonly var pos = ref ntt.Get<PositionComponent>();
            Game.Grids[pos.Map].Remove(in ntt);
            
            if(ntt.Type != EntityType.Player)
                PixelWorld.Destroy(in ntt);
            else
                ntt.Remove<DeathTagComponent>();
        }
    }
}