using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Helpers;
using MagnumOpus.Squiggly;

namespace MagnumOpus.Systems
{
    public sealed class ViewportSystem : NttSystem<PositionComponent, ViewportComponent, ViewportUpdateTagComponent>
    {
        public ViewportSystem() : base("Viewport", threads: Environment.ProcessorCount) { }
        public override void Update(in NTT ntt, ref PositionComponent pos, ref ViewportComponent vwp, ref ViewportUpdateTagComponent _)
        {
            ntt.Remove<ViewportUpdateTagComponent>();

            vwp.Viewport.X = (int)(pos.Position.X - (vwp.Viewport.Width / 2));
            vwp.Viewport.Y = (int)(pos.Position.Y - (vwp.Viewport.Height / 2));

            vwp.EntitiesVisibleLast.Clear();

            foreach (var e in vwp.EntitiesVisible)
                vwp.EntitiesVisibleLast.Add(e);
            vwp.EntitiesVisible.Clear();

            Collections.SpatialHashs[pos.Map].GetVisibleEntities(ref vwp);

            if (IsLogging)
                Logger.Debug("{ntt} has {visibleCount} visible entities", ntt, vwp.EntitiesVisible.Count);

            if (ntt.Type != EntityType.Player || vwp.EntitiesVisible.Count == 0)
                return;

            foreach (var b in vwp.EntitiesVisible)
            {
                if (b.Has<DeathTagComponent>())
                    continue;

                if (b.Has<BrainComponent>())
                {
                    ref var brn = ref b.Get<BrainComponent>();
                    if (brn.State == Enums.BrainState.Idle)
                    {
                        brn.State = Enums.BrainState.WakingUp;
                        if (IsLogging)
                            Logger.Debug("{ntt} is waking up '{b}' due to distance", ntt, b);
                    }
                }

                if (vwp.EntitiesVisibleLast.Contains(b))
                    continue;

                b.Set<ViewportUpdateTagComponent>();

                NetworkHelper.FullSync(in ntt, in b);
                NetworkHelper.FullSync(in b, in b);
            }
        }
    }
}