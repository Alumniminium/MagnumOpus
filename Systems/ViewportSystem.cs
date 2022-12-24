using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Networking;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class ViewportSystem : PixelSystem<PositionComponent, ViewportComponent>
    {
        public ViewportSystem() : base("Viewport System", threads: 1) { }
        protected override bool MatchesFilter(in PixelEntity ntt) => base.MatchesFilter(in ntt);

        public override void Update(in PixelEntity ntt, ref PositionComponent pos, ref ViewportComponent vwp)
        {
            if (pos.ChangedTick != PixelWorld.Tick)
                return;

            vwp.Viewport.X = pos.Position.X - vwp.Viewport.Width / 2;
            vwp.Viewport.Y = pos.Position.Y - vwp.Viewport.Height / 2;

            vwp.EntitiesVisibleLast.Clear();
            vwp.EntitiesVisibleLast.AddRange(vwp.EntitiesVisible);
            vwp.EntitiesVisible.Clear();

            Game.Grids[pos.Map].GetVisibleEntities(ref vwp);
            // FConsole.WriteLine($"[{nameof(ViewportSystem)}] {ntt.Id} -> {vwp.EntitiesVisible.Count} entities visible");

            if (ntt.Type != EntityType.Player)
                return;

            for (var i = 0; i < vwp.EntitiesVisible.Count; i++)
            {
                var b = vwp.EntitiesVisible[i];

                if(b.Has<BrainComponent>())
                {
                    ref var brn = ref b.Get<BrainComponent>();
                    if(brn.State == Enums.BrainState.Idle)
                        brn.State = Enums.BrainState.WakingUp;
                }

                if(vwp.EntitiesVisibleLast.Contains(b))
                    continue;
                
                NetworkHelper.FullSync(in ntt, in b);
            }
        }
    }
}