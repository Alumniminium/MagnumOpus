using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.Helpers;
using MagnumOpus.Squiggly;
using NttECS.ECS;

namespace MagnumOpus.Systems;

public sealed class ViewportSystem : NttSystem<PositionComponent, ViewportComponent, ViewportUpdateTagComponent>
{
    public ViewportSystem() : base("Viewport", threads: 1, log: false) { }

    // Manages entity visibility and viewport calculations for spatial awareness. Updates what
    // entities can see each other, tracks visibility changes, and handles AI activation when
    // players approach. Centers viewport on entity position, queries spatial hash for visible
    // entities, and synchronizes new visibility between entities for network updates.
    public override void Update(in NTT ntt, ref PositionComponent pos, ref ViewportComponent vwp, ref ViewportUpdateTagComponent _)
    {
        // Clean up the update tag component first
        ntt.Remove<ViewportUpdateTagComponent>();

        // === UPDATE VIEWPORT BOUNDS ===
        // Center viewport on entity's current position
        var viewport = vwp.Viewport;
        viewport.X = (int)(pos.Position.X - (viewport.Width / 2));
        viewport.Y = (int)(pos.Position.Y - (viewport.Height / 2));
        vwp.Viewport = viewport;

        // === TRACK VISIBILITY CHANGES ===
        // Save current visible entities as "last visible" for comparison
        vwp.EntitiesVisibleLast.Clear();
        foreach (var entity in vwp.EntitiesVisible)
            vwp.EntitiesVisibleLast.Add(entity);
        vwp.EntitiesVisible.Clear();

        // === QUERY SPATIAL HASH FOR VISIBLE ENTITIES ===
        Collections.SpatialHashes[pos.Map].GetVisibleEntities(ref vwp);

        if (IsLogging)
            FConsole.WriteLine("{ntt} has {count} visible entities", ntt, vwp.EntitiesVisible.Count);

        // Only process AI activation and synchronization for players
        if (ntt.NotPlayer())
            return;

        // === PROCESS NEWLY VISIBLE ENTITIES ===
        foreach (var visibleEntity in vwp.EntitiesVisible)
        {
            if (visibleEntity == ntt)
                continue;

            // Skip dead entities
            if (visibleEntity.Has<DeathTagComponent>())
                continue;

            // === ACTIVATE AI WHEN PLAYER APPROACHES ===
            if (visibleEntity.Has<BrainComponent>())
            {
                ref var brain = ref visibleEntity.Get<BrainComponent>();
                if (brain.State == Enums.BrainState.Idle)
                {
                    brain.State = Enums.BrainState.WakingUp;
                    if (IsLogging)
                        FConsole.WriteLine("{player} is waking up AI entity {entity}", ntt, visibleEntity);
                }
            }

            // Skip entities that were already visible (no sync needed)
            if (vwp.EntitiesVisibleLast.Contains(visibleEntity))
                continue;

            // === SYNCHRONIZE NEW VISIBILITY ===
            // Trigger viewport update for newly visible entity
            visibleEntity.Set<ViewportUpdateTagComponent>();
            NetworkHelper.FullSync(in ntt, in visibleEntity);
        }
    }
}