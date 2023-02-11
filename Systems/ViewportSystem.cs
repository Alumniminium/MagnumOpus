using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Networking;
using MagnumOpus.Networking.Packets;

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
            vwp.EntitiesVisibleLast.AddRange(vwp.EntitiesVisible);
            vwp.EntitiesVisible.Clear();

            Game.Grids[pos.Map].Move(in ntt, ref pos);
            Game.Grids[pos.Map].GetVisibleEntities(ref vwp);

            if (ntt.Type != EntityType.Player)
                return;

            for (var i = 0; i < vwp.EntitiesVisible.Count; i++)
            {
                var b = vwp.EntitiesVisible[i];

                if (b.Has<DeathTagComponent>())
                    continue;

                if (b.Has<ViewportComponent>() && !b.Get<ViewportComponent>().EntitiesVisible.Contains(ntt))
                    b.Get<ViewportComponent>().EntitiesVisible.Add(ntt);

                if (b.Has<BrainComponent>() && b.Get<BrainComponent>().State == Enums.BrainState.Idle)
                    b.Get<BrainComponent>().State = Enums.BrainState.WakingUp;

                if (vwp.EntitiesVisibleLast.Contains(b))
                    continue;

                NetworkHelper.FullSync(in ntt, in b);
                NetworkHelper.FullSync(in b, in b);

                if (b.Has<JumpComponent>())
                {
                    var packet = MsgAction.CreateJump(in ntt, in b.Get<JumpComponent>());
                    ntt.NetSync(ref packet, true);
                }

                if (b.Has<WalkComponent>())
                {
                    var packet = MsgWalk.Create(ntt.NetId, b.Get<WalkComponent>().Direction, b.Get<WalkComponent>().IsRunning);
                    ntt.NetSync(ref packet, true);
                }
            }
        }
    }
}