using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Networking;
using MagnumOpus.Networking.Packets;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class ViewportSystem : NttSystem<PositionComponent, ViewportComponent>
    {
        public ViewportSystem() : base("Viewport", threads: 12) { }
        protected override bool MatchesFilter(in NTT ntt) => base.MatchesFilter(in ntt);

        public override void Update(in NTT ntt, ref PositionComponent pos, ref ViewportComponent vwp)
        {
            if (pos.ChangedTick != NttWorld.Tick)
                return;

            vwp.Viewport.X = pos.Position.X - vwp.Viewport.Width / 2;
            vwp.Viewport.Y = pos.Position.Y - vwp.Viewport.Height / 2;

            vwp.EntitiesVisibleLast.Clear();
            vwp.EntitiesVisibleLast.AddRange(vwp.EntitiesVisible);
            vwp.EntitiesVisible.Clear();

            Game.Grids[pos.Map].Move(in ntt, ref pos);
            Game.Grids[pos.Map].GetVisibleEntities(ref vwp);

            // FConsole.WriteLine($"[{nameof(ViewportSystem)}] {ntt.Id} -> {vwp.EntitiesVisible.Count} entities visible");

            if (ntt.Type != EntityType.Player)
                return;

            for (var i = 0; i < vwp.EntitiesVisible.Count; i++)
            {
                var b = vwp.EntitiesVisible[i];

                if(b.Has<DeathTagComponent>())
                    continue;

                if(b.Has<ViewportComponent>())
                {
                    ref readonly var bvwp = ref b.Get<ViewportComponent>();
                    if (!bvwp.EntitiesVisible.Contains(ntt))
                        bvwp.EntitiesVisible.Add(ntt);
                }

                if(b.Has<BrainComponent>())
                {
                    ref var brn = ref b.Get<BrainComponent>();
                    if(brn.State == Enums.BrainState.Idle)
                        brn.State = Enums.BrainState.WakingUp;
                }

                if(vwp.EntitiesVisibleLast.Contains(b))
                    continue;

                NetworkHelper.FullSync(in ntt, in b);
                NetworkHelper.FullSync(in b, in b);

                if(b.Has<JumpComponent>())
                {
                    ref readonly var jmp = ref b.Get<JumpComponent>();
                    var packet = MsgAction.CreateJump(in ntt, in jmp);
                    ntt.NetSync(ref packet, true);
                }

                if(b.Has<WalkComponent>())
                {
                    ref readonly var wlk = ref b.Get<WalkComponent>();
                    var packet = MsgWalk.Create(ntt.NetId, wlk.Direction, wlk.IsRunning);
                    ntt.NetSync(ref packet, true);
                }
            }
        }
    }
}