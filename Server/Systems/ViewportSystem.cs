using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Helpers;
using MagnumOpus.Squiggly;
using NttECS.ECS;

namespace MagnumOpus.Systems
{
    /// <summary>
    /// Manages entity visibility and viewport calculations for spatial awareness.
    /// Updates what entities can see each other and handles AI activation when players approach.
    /// </summary>
    /// <example>
    /// // Triggered when entity moves or ViewportUpdateTagComponent is added
    /// entity.Set&lt;ViewportUpdateTagComponent&gt;(); // Triggers viewport recalculation
    /// </example>
    public sealed class ViewportSystem : NttSystem<PositionComponent, ViewportComponent, ViewportUpdateTagComponent>
    {
        /// <summary>
        /// Initializes the ViewportSystem with multi-threaded processing capabilities.
        /// </summary>
        public ViewportSystem() : base("Viewport", threads: 1) { }
        
        /// <summary>
        /// Updates an entity's viewport, calculates visible entities, and handles AI activation.
        /// </summary>
        /// <param name="ntt">The entity whose viewport needs updating</param>
        /// <param name="pos">Entity's position component for viewport centering</param>
        /// <param name="vwp">Viewport component containing visibility data</param>
        /// <param name="_">Update tag component (consumed and removed)</param>
        /// <example>
        /// // Called automatically when entity has ViewportUpdateTagComponent
        /// // Updates what the entity can see and activates nearby AI entities
        /// </example>
        public override void Update(in NTT ntt, ref PositionComponent pos, ref ViewportComponent vwp, ref ViewportUpdateTagComponent _)
        {
            // ntt.Remove<ViewportUpdateTagComponent>();

            var viewport = vwp.Viewport;
            viewport.X = (int)(pos.Position.X - (viewport.Width / 2));
            viewport.Y = (int)(pos.Position.Y - (viewport.Height / 2));
            vwp.Viewport = viewport;

            vwp.EntitiesVisibleLast.Clear();

            foreach (var entity in vwp.EntitiesVisible)
                vwp.EntitiesVisibleLast.Add(entity);
            vwp.EntitiesVisible.Clear();

            Collections.SpatialHashes[pos.Map].GetVisibleEntities(ref vwp);

            if (IsLogging)
                FConsole.WriteLine("{ntt} has {visibleCount} visible entities", ntt, vwp.EntitiesVisible.Count);

            if (!ntt.IsPlayer())
                return;

            foreach (var visibleEntity in vwp.EntitiesVisible)
            {
                if (visibleEntity.Has<DeathTagComponent>())
                    continue;

                if (visibleEntity.Has<BrainComponent>())
                {
                    ref var brain = ref visibleEntity.Get<BrainComponent>();
                    if (brain.State == Enums.BrainState.Idle)
                    {
                        brain.State = Enums.BrainState.WakingUp;
                        if (IsLogging)
                            FConsole.WriteLine("{ntt} is waking up '{visibleEntity}' due to distance", ntt, visibleEntity);
                    }
                }

                if (vwp.EntitiesVisibleLast.Contains(visibleEntity))
                    continue;

                visibleEntity.Set<ViewportUpdateTagComponent>();

                NetworkHelper.FullSync(in ntt, in visibleEntity);
                NetworkHelper.FullSync(in visibleEntity, in ntt);
            }
        }
    }
}