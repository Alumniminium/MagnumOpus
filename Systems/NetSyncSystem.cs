using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class NetSyncSystem : PixelSystem<NetSyncComponent>
    {
        public NetSyncSystem() : base("NetSync System", threads: 1) { }

        protected override bool MatchesFilter(in PixelEntity ntt) => base.MatchesFilter(ntt);

        public override void Update(in PixelEntity ntt, ref NetSyncComponent c1)
        {
            ref readonly var syn = ref ntt.Get<NetSyncComponent>();
            if (syn.Fields.HasFlag(SyncThings.Walk) && ntt.Has<WalkComponent>())
            {
                ref readonly var x = ref ntt.Get<WalkComponent>();
                var pkt = MsgWalk.Create(ntt.NetId, x.Direction, x.IsRunning);
                ntt.NetSync(ref pkt, true);
                ntt.Remove<WalkComponent>();
            }
            if (ntt.Has<HealthComponent>())
            {
                ref readonly var x = ref ntt.Get<HealthComponent>();
                if (x.ChangedTick == PixelWorld.Tick)
                {
                    var pkt = MsgUserAttrib.Create(ntt.NetId, (ulong)x.Health, MsgUserAttribType.Health);
                    ntt.NetSync(ref pkt, true);
                }
            }
            if (ntt.Has<ManaComponent>())
            {
                ref readonly var x = ref ntt.Get<ManaComponent>();
                if (x.ChangedTick == PixelWorld.Tick)
                {
                    var pkt = MsgUserAttrib.Create(ntt.NetId, (ulong)x.Mana, MsgUserAttribType.Mana);
                    ntt.NetSync(ref pkt, true);
                }
            }
        }
    }
}