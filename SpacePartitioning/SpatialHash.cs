using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.CompilerServices;
using MagnumOpus.Components;
using MagnumOpus.ECS;

namespace MagnumOpus.SpacePartitioning
{
    public class SpatialHash
    {
        private readonly int cellSize;
        private readonly Dictionary<int, List<NTT>> Hashtbl;

        public SpatialHash(int cellSize)
        {
            this.cellSize = cellSize;
            Hashtbl = new Dictionary<int, List<NTT>>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(NTT entity, Vector2 pos)
        {
            var hash = GetHash(pos);
            if (!Hashtbl.TryGetValue(hash, out var list))
            {
                list = new List<NTT>();
                Hashtbl.Add(hash, list);
            }
            list.Add(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(NTT entity, Vector2 pos)
        {
            var hash = GetHash(pos);
            if (Hashtbl.TryGetValue(hash, out var bucket))
                bucket.Remove(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Move(NTT ntt, PositionComponent pos)
        {
            Remove(ntt, pos.LastPosition);
            Add(ntt, pos.Position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetVisibleEntities(ref ViewportComponent vwp)
        {
            var cx = vwp.Viewport.X + (vwp.Viewport.Width / 2);
            var cy = vwp.Viewport.Y + (vwp.Viewport.Height / 2);

            var minX = (int)((vwp.Viewport.Left - (vwp.Viewport.Width / 2)) / cellSize);
            var maxX = (int)((vwp.Viewport.Right + (vwp.Viewport.Width / 2)) / cellSize);
            var minY = (int)((vwp.Viewport.Top - (vwp.Viewport.Height / 2)) / cellSize);
            var maxY = (int)((vwp.Viewport.Bottom + (vwp.Viewport.Height / 2)) / cellSize);

            for (var x = minX; x <= maxX; x++)
            {
                for (var y = minY; y <= maxY; y++)
                {
                    var hash = GetHash(new Vector2(x * cellSize, y * cellSize));

                    if (!Hashtbl.TryGetValue(hash, out var entities))
                        continue;

                    foreach (var ntt in entities)
                    {
                        ref readonly var pos = ref ntt.Get<PositionComponent>();
                        var distanceSquared = Vector2.DistanceSquared(pos.Position, new Vector2(cx, cy));

                        if (distanceSquared <= 324f)
                            vwp.EntitiesVisible.Add(ntt);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHash(Vector2 position)
        {
            var scaled = position / cellSize;
            var x = (int)scaled.X;
            var y = (int)scaled.Y;

            return (x * 73856093) ^ (y * 19349663);
        }
    }
}