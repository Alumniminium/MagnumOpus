using MagnumOpus.Components;
using MagnumOpus.ECS;
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
                Collections.SpatialHashs[shr.Map].Remove(ntt, shr.Position);
            }
            else if (shr.Type == SpacialHashUpdatType.Add)
            {
                if (!Collections.SpatialHashs.ContainsKey(shr.Map))
                    Collections.SpatialHashs[shr.Map] = new SpatialHash(10);

                Collections.SpatialHashs[shr.Map].Add(ntt, shr.Position);
            }
            else
            {
                Collections.SpatialHashs[shr.Map].Move(ntt, ntt.Get<PositionComponent>());
            }
            ntt.Remove<SpatialHashUpdateComponent>();
        }
    }
}
