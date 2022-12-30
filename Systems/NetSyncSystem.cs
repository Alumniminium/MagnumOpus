using MagnumOpus.ECS;
using MagnumOpus.Components;
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
                ref readonly var wlk = ref ntt.Get<WalkComponent>();
                var pkt = MsgWalk.Create(ntt.NetId, wlk.Direction, wlk.IsRunning);
                ntt.NetSync(ref pkt, true);
                ntt.Remove<WalkComponent>();
            }
            if(syn.Fields.HasFlag(SyncThings.Jump) && ntt.Has<JumpComponent>())
            {
                ref readonly var jmp = ref ntt.Get<JumpComponent>();
                var msg = MsgAction.CreateJump(in ntt, in jmp);
                ntt.NetSync(ref msg, true);
                ntt.Remove<JumpComponent>();
            }
        }
    }
}