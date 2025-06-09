using MagnumOpus.Components;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;
using MagnumOpus.SpacePartitioning;
using MagnumOpus.Squiggly;
using NttECS.ECS;

namespace MagnumOpus.Systems;

public sealed class SpatialHashSystem : NttSystem<SpatialHashUpdateComponent>
{
    public SpatialHashSystem() : base("Remove Spatial Hash", threads: 1 / 4) { }

    // Manages spatial hash data structure updates for efficient entity queries and collision
    // detection. Handles entity additions, removals, and position updates across maps with
    // automatic hash management. Creates new spatial hashes for maps as needed and broadcasts
    // death messages when entities are removed from the spatial system.
    public override void Update(in NTT ntt, ref SpatialHashUpdateComponent spatialUpdate)
    {
        switch (spatialUpdate.Type)
        {
            case SpacialHashUpdatType.Remove:
                {
                    // === REMOVE ENTITY FROM SPATIAL HASH ===
                    // Broadcast death message and remove from spatial system
                    var deathMessage = MsgInteract.Create(ntt, ntt, Enums.MsgInteractType.Death, 0);
                    ntt.NetSync(ref deathMessage, broadcast: true);
                    Collections.SpatialHashes[spatialUpdate.Map].Remove(ntt, spatialUpdate.Position);
                    break;
                }

            case SpacialHashUpdatType.Add:
                // === ADD ENTITY TO SPATIAL HASH ===
                // Create spatial hash for map if it doesn't exist
                if (!Collections.SpatialHashes.ContainsKey(spatialUpdate.Map))
                    Collections.SpatialHashes[spatialUpdate.Map] = new SpatialHash();

                Collections.SpatialHashes[spatialUpdate.Map].Add(ntt, spatialUpdate.Position);
                break;
            default:
                // === MOVE ENTITY IN SPATIAL HASH ===
                if (spatialUpdate.LastMap != spatialUpdate.Map)
                {
                    // Cross-map movement: remove from old map, add to new map
                    Collections.SpatialHashes[spatialUpdate.LastMap].Remove(ntt, spatialUpdate.LastPosition);

                    if (!Collections.SpatialHashes.ContainsKey(spatialUpdate.Map))
                        Collections.SpatialHashes[spatialUpdate.Map] = new SpatialHash();

                    Collections.SpatialHashes[spatialUpdate.Map].Add(ntt, spatialUpdate.Position);
                }
                else
                {
                    // Same-map movement: use optimized move operation
                    Collections.SpatialHashes[spatialUpdate.Map].Move(ntt, ntt.Get<PositionComponent>());
                }
                break;
        }

        // Clean up the spatial update component
        ntt.Remove<SpatialHashUpdateComponent>();
    }
}
