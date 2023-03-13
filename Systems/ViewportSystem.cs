using MagnumOpus.Components.AI;
using MagnumOpus.Components.Death;
using MagnumOpus.Components.Entity;
using MagnumOpus.Components.Location;
using MagnumOpus.ECS;
using MagnumOpus.Helpers;
using MagnumOpus.Squiggly;

namespace MagnumOpus.Systems
{
    public sealed class ViewportSystem : NttSystem<PositionComponent, ViewportComponent>
    {
        public ViewportSystem() : base("Viewport", threads: Environment.ProcessorCount) { }
        public override void Update(in NTT ntt, ref PositionComponent pos, ref ViewportComponent vwp)
        {
            if (!vwp.Dirty)
                return;

            vwp.Dirty = false;

            vwp.Viewport.X = pos.Position.X - (vwp.Viewport.Width / 2);
            vwp.Viewport.Y = pos.Position.Y - (vwp.Viewport.Height / 2);

            vwp.EntitiesVisibleLast.Clear();
            foreach (var e in vwp.EntitiesVisible)
                _ = vwp.EntitiesVisibleLast.TryAdd(e.Key, e.Value);

            vwp.EntitiesVisible.Clear();
            Collections.SpatialHashs[pos.Map].Move(ntt, ref pos);
            Collections.SpatialHashs[pos.Map].GetVisibleEntities(ref vwp);

            if (IsLogging)
                Logger.Debug("{ntt} has {visibleCount} visible entities", ntt, vwp.EntitiesVisible.Count);

            if (ntt.Type != EntityType.Player)
                return;

            foreach (var kvp in vwp.EntitiesVisible)
            {
                var b = kvp.Value;
                if (b.Has<DeathTagComponent>())
                    continue;

                if (b.Has<ViewportComponent>())
                {
                    ref readonly var bvwp = ref b.Get<ViewportComponent>();
                    _ = bvwp.EntitiesVisible.TryAdd(ntt.Id, ntt);
                }

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

                if (vwp.EntitiesVisibleLast.ContainsKey(b.Id))
                    continue;

                NetworkHelper.FullSync(in ntt, in b);
                NetworkHelper.FullSync(in b, in b);

                // if (b.Has<JumpComponent>())
                // {
                //     ref readonly var jmp = ref b.Get<JumpComponent>();
                //     var packet = MsgAction.CreateJump(in b, in jmp);
                //     ntt.NetSync(ref packet, true);
                // }

                // if (b.Has<WalkComponent>())
                // {
                //     ref readonly var wlk = ref b.Get<WalkComponent>();
                //     var packet = MsgWalk.Create(b.Id, wlk.Direction, wlk.IsRunning);
                //     ntt.NetSync(ref packet, true);
                // }
            }
        }
    }
}