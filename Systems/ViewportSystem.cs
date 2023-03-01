using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Networking;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Squiggly;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class ViewportSystem : NttSystem<PositionComponent, ViewportComponent>
    {
        public ViewportSystem() : base("Viewport", threads: Environment.ProcessorCount) { }
        public override void Update(in NTT ntt, ref PositionComponent pos, ref ViewportComponent vwp)
        {
            if (pos.ChangedTick != NttWorld.Tick)
                return;

            vwp.Viewport.X = pos.Position.X - vwp.Viewport.Width / 2;
            vwp.Viewport.Y = pos.Position.Y - vwp.Viewport.Height / 2;

            vwp.EntitiesVisibleLast.Clear();
            foreach(var e in vwp.EntitiesVisible)
                vwp.EntitiesVisibleLast.TryAdd(e.Key,e.Value);

            vwp.EntitiesVisible.Clear();
            
            Collections.SpatialHashs[pos.Map].Remove(in ntt);
            Collections.SpatialHashs[pos.Map].Add(in ntt);
            Collections.SpatialHashs[pos.Map].GetVisibleEntities(ref vwp);


            // FConsole.WriteLine($"[{nameof(ViewportSystem)}] {ntt.Id} -> {vwp.EntitiesVisible.Count} entities visible");

            if (ntt.Type != EntityType.Player)
                return;

            
                foreach (var kvp in vwp.EntitiesVisible)
                {
                    var b = kvp.Value;
                if (b.Has<DeathTagComponent>())
                    continue;

                if(b.Has<ViewportComponent>())
                {
                    ref readonly var bvwp = ref b.Get<ViewportComponent>();
                    bvwp.EntitiesVisible.TryAdd(ntt.Id, ntt);
                }

                if(b.Has<BrainComponent>())
                {
                    ref var brn = ref b.Get<BrainComponent>();
                    if(brn.State == Enums.BrainState.Idle)
                        brn.State = Enums.BrainState.WakingUp;
                }

                if(vwp.EntitiesVisibleLast.ContainsKey(b.Id))
                    continue;

                NetworkHelper.FullSync(in ntt, in b);
                NetworkHelper.FullSync(in b, in b);

                if(b.Has<JumpComponent>())
                {
                    ref readonly var jmp = ref b.Get<JumpComponent>();
                    var packet = MsgAction.CreateJump(in b, in jmp);
                    ntt.NetSync(ref packet, true);
                }

                if(b.Has<WalkComponent>())
                {
                    ref readonly var wlk = ref b.Get<WalkComponent>();
                    var packet = MsgWalk.Create(b.NetId, wlk.Direction, wlk.IsRunning);
                    ntt.NetSync(ref packet, true);
                }
            }
        }
    }
}