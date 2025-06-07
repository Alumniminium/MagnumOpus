using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Networking.Packets;
using MagnumOpus.SpacePartitioning;
using MagnumOpus.Squiggly;

namespace MagnumOpus.Systems
{
    /// <summary>
    /// Manages spatial hash data structure updates for efficient entity queries and collision detection.
    /// Handles entity additions, removals, and position updates across maps with automatic hash management.
    /// </summary>
    public sealed class SpatialHashSystem : NttSystem<SpatialHashUpdateComponent>
    {
        /// <summary>
        /// Initializes the SpatialHashSystem with multi-threaded processing for spatial updates.
        /// </summary>
        public SpatialHashSystem() : base("Remove Spatial Hash", threads: Environment.ProcessorCount / 4) { }
        
        /// <summary>
        /// Processes spatial hash updates including entity additions, removals, and map transfers.
        /// </summary>
        /// <param name="ntt">The entity being updated in the spatial hash</param>
        /// <param name="spatialUpdate">Spatial hash update component containing operation details</param>
        public override void Update(in NTT ntt, ref SpatialHashUpdateComponent spatialUpdate)
        {
            if (spatialUpdate.Type == SpacialHashUpdatType.Remove)
            {
                var deathMessage = MsgInteract.Create(ntt, ntt, Enums.MsgInteractType.Death, 0);
                ntt.NetSync(ref deathMessage, true);
                Collections.SpatialHashes[spatialUpdate.Map].Remove(ntt, spatialUpdate.Position);
            }
            else if (spatialUpdate.Type == SpacialHashUpdatType.Add)
            {
                if (!Collections.SpatialHashes.ContainsKey(spatialUpdate.Map))
                    Collections.SpatialHashes[spatialUpdate.Map] = new SpatialHash();

                Collections.SpatialHashes[spatialUpdate.Map].Add(ntt, spatialUpdate.Position);
            }
            else
            {
                if (spatialUpdate.LastMap != spatialUpdate.Map)
                {
                    Collections.SpatialHashes[spatialUpdate.LastMap].Remove(ntt, spatialUpdate.LastPosition);
                    if (!Collections.SpatialHashes.ContainsKey(spatialUpdate.Map))
                        Collections.SpatialHashes[spatialUpdate.Map] = new SpatialHash();
                    Collections.SpatialHashes[spatialUpdate.Map].Add(ntt, spatialUpdate.Position);
                }
                else
                    Collections.SpatialHashes[spatialUpdate.Map].Move(ntt, ntt.Get<PositionComponent>());
            }
            ntt.Remove<SpatialHashUpdateComponent>();
        }
    }
}
