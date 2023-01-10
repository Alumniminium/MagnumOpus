using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    // public sealed class HealthSystem : PixelSystem<HealthComponent>
    // {
    //     public HealthSystem() : base("Health System", threads: 1) { }

    //     public override void Update(in PixelEntity ntt, ref HealthComponent hlt)
    //     {
    //         if (hlt.ChangedTick != PixelWorld.Tick)
    //             return;

    //         var pkt = MsgUserAttrib.Create(ntt.NetId, (ulong)hlt.Health, MsgUserAttribType.Health);
    //         ntt.NetSync(ref pkt, true);
    //     }
    // }
    // public sealed class ManaSystem : PixelSystem<ManaComponent>
    // {
    //     public ManaSystem() : base("Mana System", threads: 1) { }

    //     public override void Update(in PixelEntity ntt, ref ManaComponent mna)
    //     {
    //         if (mna.ChangedTick != PixelWorld.Tick)
    //             return;

    //         var pkt = MsgUserAttrib.Create(ntt.NetId, (ulong)mna.Mana, MsgUserAttribType.Health);
    //         ntt.NetSync(ref pkt, true);
    //     }
    // }
}