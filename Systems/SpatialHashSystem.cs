using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Networking.Packets;
using MagnumOpus.SpacePartitioning;
using MagnumOpus.Squiggly;

namespace MagnumOpus.Systems
{
    public sealed class SpatialHashSystem : NttSystem<SpatialHashUpdateComponent>
    {
        public SpatialHashSystem() : base("Remove Spatial Hash", threads: 1) { }
        public override void Update(in NTT ntt, ref SpatialHashUpdateComponent shr)
        {
            if (shr.Type == SpacialHashUpdatType.Remove)
            {
                var msg = MsgInteract.Create(ntt, ntt, Enums.MsgInteractType.Death, 0);
                ntt.NetSync(ref msg, true);
                Collections.SpatialHashes[shr.Map].Remove(ntt, shr.Position);
            }
            else if (shr.Type == SpacialHashUpdatType.Add)
            {
                if (!Collections.SpatialHashes.ContainsKey(shr.Map))
                    Collections.SpatialHashes[shr.Map] = new SpatialHash();

                Collections.SpatialHashes[shr.Map].Add(ntt, shr.Position);
            }
            else
            {
                if (shr.LastMap != shr.Map)
                {
                    Collections.SpatialHashes[shr.LastMap].Remove(ntt, shr.LastPosition);
                    if (!Collections.SpatialHashes.ContainsKey(shr.Map))
                        Collections.SpatialHashes[shr.Map] = new SpatialHash();
                    Collections.SpatialHashes[shr.Map].Add(ntt, shr.Position);
                }
                else
                    Collections.SpatialHashes[shr.Map].Move(ntt, ntt.Get<PositionComponent>());
            }
            ntt.Remove<SpatialHashUpdateComponent>();
        }
    }
}
